const { DataApi } = require("@unity-services/cloud-save-1.4");

const PROGRESS_KEY = "idle_progress";
const PENDING_PROGRESS_KEY = "idle_progress_pending";
const CURRENT_SCHEMA_VERSION = 1;

module.exports = async ({ context, logger }) => {
  const { projectId, playerId } = context;
  const cloudSaveApi = new DataApi({ accessToken: context.accessToken });
  const serverUnixTime = Math.floor(Date.now() / 1000);
  const progressJson = await loadPendingProgress(cloudSaveApi, projectId, playerId);
  const currentProgressJson = await loadCurrentProgress(cloudSaveApi, projectId, playerId);

  if (!progressJson || typeof progressJson !== "string") {
    throw new Error(`Pending progress is required. Save ${PENDING_PROGRESS_KEY} before calling this script.`);
  }

  let snapshot;

  try {
    snapshot = JSON.parse(progressJson);
  } catch (err) {
    throw new Error("progressJson is not valid JSON.");
  }

  if (!snapshot || snapshot.schemaVersion < 1 || snapshot.schemaVersion > CURRENT_SCHEMA_VERSION) {
    throw new Error(`Unsupported progress schema: ${snapshot?.schemaVersion}`);
  }

  let currentSnapshot = null;

  if (currentProgressJson) {
    try {
      currentSnapshot = JSON.parse(currentProgressJson);
    } catch (err) {
      currentSnapshot = null;
    }
  }

  const submittedSavedAtUnixTime = normalizeSubmittedSavedAt(
    snapshot.clientSavedAtUnixTime || snapshot.savedAtUnixTime,
    serverUnixTime);
  const currentSavedAtUnixTime = clampInt(
    currentSnapshot?.clientSavedAtUnixTime || currentSnapshot?.savedAtUnixTime,
    0,
    serverUnixTime);

  if (currentSavedAtUnixTime > submittedSavedAtUnixTime) {
    return {
      saved: false,
      stale: true,
      savedAtUnixTime: currentSavedAtUnixTime,
      serverUnixTime,
    };
  }

  normalizeSnapshot(snapshot, serverUnixTime, submittedSavedAtUnixTime);

  try {
    await cloudSaveApi.setItem(projectId, playerId, {
      key: PROGRESS_KEY,
      value: JSON.stringify(snapshot),
    });

    return {
      saved: true,
      savedAtUnixTime: snapshot.savedAtUnixTime,
      serverUnixTime,
    };
  } catch (err) {
    logger.error("Failed to save Idle Space progress", { message: err.message });
    throw err;
  }
};

module.exports.params = {};

async function loadPendingProgress(cloudSaveApi, projectId, playerId) {
  try {
    const response = await cloudSaveApi.getItems(projectId, playerId, PENDING_PROGRESS_KEY);
    const results = response?.data?.results || [];
    const item = results.find((entry) => entry.key === PENDING_PROGRESS_KEY) || results[0];

    if (!item || item.value === undefined || item.value === null) {
      return "";
    }

    return typeof item.value === "string" ? item.value : JSON.stringify(item.value);
  } catch (err) {
    return "";
  }
}

async function loadCurrentProgress(cloudSaveApi, projectId, playerId) {
  try {
    const response = await cloudSaveApi.getItems(projectId, playerId, PROGRESS_KEY);
    const results = response?.data?.results || [];
    const item = results.find((entry) => entry.key === PROGRESS_KEY) || results[0];

    if (!item || item.value === undefined || item.value === null) {
      return "";
    }

    return typeof item.value === "string" ? item.value : JSON.stringify(item.value);
  } catch (err) {
    return "";
  }
}

function normalizeSnapshot(snapshot, serverUnixTime, savedAtUnixTime) {
  snapshot.schemaVersion = CURRENT_SCHEMA_VERSION;
  snapshot.savedAtUnixTime = savedAtUnixTime;
  snapshot.clientSavedAtUnixTime = savedAtUnixTime;

  const gameData = snapshot.gameData || {};
  gameData.resources = Array.isArray(gameData.resources) ? gameData.resources : [];
  gameData.shuttleStates = Array.isArray(gameData.shuttleStates) ? gameData.shuttleStates : [];

  gameData.resources.forEach((resource) => {
    resource.amount = clampInt(resource.amount, 0, Number.MAX_SAFE_INTEGER);
  });

  gameData.shuttleStates.forEach((shuttle) => {
    shuttle.dockedOre = clampInt(shuttle.dockedOre, 0, Number.MAX_SAFE_INTEGER);
    shuttle.loadingOre = clampInt(shuttle.loadingOre, 0, Number.MAX_SAFE_INTEGER);
    shuttle.loadingTargetOre = clampInt(shuttle.loadingTargetOre, shuttle.loadingOre || 0, Number.MAX_SAFE_INTEGER);
    shuttle.deliveringOre = clampInt(shuttle.deliveringOre, 0, Number.MAX_SAFE_INTEGER);
    shuttle.loadingCooldownRemaining = clampFloat(shuttle.loadingCooldownRemaining, 0, 3600);
    shuttle.sendCooldownRemaining = clampFloat(shuttle.sendCooldownRemaining, 0, 3600);
  });

  gameData.shuttleCount = clampInt(gameData.shuttleCount, 1, 3);
  gameData.shuttleAutoSendCount = clampInt(gameData.shuttleAutoSendCount, 0, gameData.shuttleCount);
  gameData.platformCapacity = clampInt(gameData.platformCapacity, 0, Number.MAX_SAFE_INTEGER);
  gameData.shuttleCapacity = clampInt(gameData.shuttleCapacity, 1, Number.MAX_SAFE_INTEGER);
  gameData.orePerClick = clampInt(gameData.orePerClick, 0, Number.MAX_SAFE_INTEGER);
  gameData.orePerSecond = clampInt(gameData.orePerSecond, 0, Number.MAX_SAFE_INTEGER);
  gameData.energyMax = clampInt(gameData.energyMax, 0, Number.MAX_SAFE_INTEGER);
  gameData.energyRegenAmount = clampInt(gameData.energyRegenAmount, 0, Number.MAX_SAFE_INTEGER);
  gameData.energyRegenInterval = clampFloat(gameData.energyRegenInterval, 0.1, 86400);
  gameData.metalPerCraft = clampInt(gameData.metalPerCraft, 0, Number.MAX_SAFE_INTEGER);
  gameData.metalOreCost = clampInt(gameData.metalOreCost, 0, Number.MAX_SAFE_INTEGER);
  gameData.metalEnergyCost = clampInt(gameData.metalEnergyCost, 0, Number.MAX_SAFE_INTEGER);
  gameData.totalOreEarned = clampInt(gameData.totalOreEarned, 0, Number.MAX_SAFE_INTEGER);

  snapshot.gameData = gameData;
  normalizeLevels(snapshot.upgradeLevels);
  normalizeLevels(snapshot.buildingLevels);
  normalizeLevels(snapshot.metaBonusLevels);
}

function normalizeSubmittedSavedAt(submittedSavedAt, serverUnixTime) {
  let savedAtUnixTime = clampInt(submittedSavedAt, 0, serverUnixTime);

  if (savedAtUnixTime <= 0) {
    savedAtUnixTime = serverUnixTime;
  }

  return savedAtUnixTime;
}

function normalizeLevels(levels) {
  if (!Array.isArray(levels)) {
    return;
  }

  levels.forEach((entry) => {
    entry.level = clampInt(entry.level, 0, 100000);
  });
}

function clampInt(value, min, max) {
  const parsed = Number.parseInt(value, 10);
  return Math.min(max, Math.max(min, Number.isFinite(parsed) ? parsed : min));
}

function clampFloat(value, min, max) {
  const parsed = Number.parseFloat(value);
  return Math.min(max, Math.max(min, Number.isFinite(parsed) ? parsed : min));
}
