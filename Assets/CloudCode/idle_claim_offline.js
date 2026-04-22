const { DataApi } = require("@unity-services/cloud-save-1.4");

const PROGRESS_KEY = "idle_progress";
const MAX_OFFLINE_SECONDS = 8 * 60 * 60;

module.exports = async ({ context, logger }) => {
  const { projectId, playerId } = context;
  const cloudSaveApi = new DataApi({ accessToken: context.accessToken });
  const serverUnixTime = Math.floor(Date.now() / 1000);

  const progressItem = await loadProgress(cloudSaveApi, projectId, playerId);

  if (!progressItem) {
    return {
      claimed: false,
      warehouseOre: 0,
      elapsedSeconds: 0,
      serverUnixTime,
    };
  }

  const snapshot = JSON.parse(progressItem);
  const lastSavedAt = Math.max(0, Number.parseInt(snapshot.savedAtUnixTime || 0, 10));
  const elapsedSeconds = Math.min(MAX_OFFLINE_SECONDS, Math.max(0, serverUnixTime - lastSavedAt));
  const gameData = snapshot.gameData || {};
  const orePerSecond = Math.max(0, Number.parseInt(gameData.orePerSecond || 0, 10));
  const autoSendCount = Math.max(0, Number.parseInt(gameData.shuttleAutoSendCount || 0, 10));

  if (elapsedSeconds <= 0 || orePerSecond <= 0 || autoSendCount <= 0) {
    snapshot.savedAtUnixTime = serverUnixTime;
    await saveProgress(cloudSaveApi, projectId, playerId, snapshot);

    return {
      claimed: true,
      warehouseOre: 0,
      elapsedSeconds,
      serverUnixTime,
    };
  }

  const shuttleCapacity = Math.max(1, Number.parseInt(gameData.shuttleCapacity || 1, 10));
  const shuttleTravelTime = Math.max(1, Number.parseFloat(gameData.shuttleTravelTimeSeconds || 60));
  const deliveryThroughputPerSecond = (autoSendCount * shuttleCapacity) / shuttleTravelTime;
  const authoritativeOrePerSecond = Math.min(orePerSecond, deliveryThroughputPerSecond);
  const warehouseOre = Math.floor(authoritativeOrePerSecond * elapsedSeconds);

  addOreToWarehouse(gameData, warehouseOre);
  snapshot.gameData = gameData;
  snapshot.savedAtUnixTime = serverUnixTime;
  await saveProgress(cloudSaveApi, projectId, playerId, snapshot);

  logger.info("Idle Space offline reward claimed", {
    playerId,
    elapsedSeconds,
    warehouseOre,
  });

  return {
    claimed: true,
    warehouseOre,
    elapsedSeconds,
    serverUnixTime,
  };
};

module.exports.params = {};

async function loadProgress(cloudSaveApi, projectId, playerId) {
  try {
    const response = await cloudSaveApi.getItems(projectId, playerId, PROGRESS_KEY);
    const results = response?.data?.results || [];
    const item = results.find((entry) => entry.key === PROGRESS_KEY) || results[0];
    return item?.value || null;
  } catch (err) {
    return null;
  }
}

async function saveProgress(cloudSaveApi, projectId, playerId, snapshot) {
  await cloudSaveApi.setItem(projectId, playerId, {
    key: PROGRESS_KEY,
    value: JSON.stringify(snapshot),
  });
}

function addOreToWarehouse(gameData, amount) {
  if (amount <= 0) {
    return;
  }

  gameData.resources = Array.isArray(gameData.resources) ? gameData.resources : [];
  let ore = gameData.resources.find((resource) => resource.resourceType === 0 || resource.resourceType === "Ore");

  if (!ore) {
    ore = {
      resourceType: 0,
      amount: 0,
    };
    gameData.resources.push(ore);
  }

  ore.amount = Math.max(0, Number.parseInt(ore.amount || 0, 10)) + amount;
}
