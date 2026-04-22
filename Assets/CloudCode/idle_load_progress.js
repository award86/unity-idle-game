const { DataApi } = require("@unity-services/cloud-save-1.4");

const PROGRESS_KEY = "idle_progress";

module.exports = async ({ context, logger }) => {
  const { projectId, playerId } = context;
  const cloudSaveApi = new DataApi({ accessToken: context.accessToken });
  const serverUnixTime = Math.floor(Date.now() / 1000);

  try {
    const response = await cloudSaveApi.getItems(projectId, playerId, PROGRESS_KEY);
    const results = response?.data?.results || [];
    const item = results.find((entry) => entry.key === PROGRESS_KEY) || results[0];

    if (!item || item.value === undefined || item.value === null) {
      return {
        hasProgress: false,
        progressJson: "",
        serverUnixTime,
      };
    }

    return {
      hasProgress: true,
      progressJson: typeof item.value === "string" ? item.value : JSON.stringify(item.value),
      serverUnixTime,
    };
  } catch (err) {
    logger.error("Failed to load Idle Space progress", { message: err.message });
    return {
      hasProgress: false,
      progressJson: "",
      serverUnixTime,
    };
  }
};

module.exports.params = {};
