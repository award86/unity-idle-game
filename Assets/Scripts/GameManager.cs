using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string OreKey = "ore";
    private const string ShuttleOreKey = "shuttleOre";
    private const string ShuttleCapacityKey = "shuttleCapacity";
    private const string ShuttleSendCooldownKey = "shuttleSendCooldown";
    private const string OrePerClickKey = "orePerClick";
    private const string OrePerSecondKey = "orePerSecond";
    private const string TotalOreEarnedKey = "totalOreEarned";
    private const string LastSaveTimeKey = "lastSaveTime";
    private const string LegacyUpgradeLevelKey = "upgradeLevel";

    [SerializeField] private UIManager uiManager;
    [SerializeField] private ShuttleConfig shuttleConfig;
    [SerializeField] private UpgradeConfig upgradeConfig;
    [SerializeField] private TemporaryBoostConfig temporaryBoostConfig;

    private GameData gameData;
    private ShuttleSystem shuttleSystem;
    private ResourceSystem resourceSystem;
    private UpgradeManager upgradeManager;
    private TimeSystem timeSystem;
    private float autoSaveTimer;
    private int pendingOfflineOre;
    private int pendingOfflineWarehouseOre;
    private int pendingOfflineShuttleOre;
    private float pendingOfflineShuttleCooldown;

    private void Awake()
    {
        LoadGame();

        shuttleSystem = new ShuttleSystem(gameData);
        resourceSystem = new ResourceSystem(gameData, shuttleSystem);
        upgradeManager = new UpgradeManager(gameData, shuttleConfig, upgradeConfig, temporaryBoostConfig);
        upgradeManager.LoadUpgradeLevels();
        upgradeManager.UpgradesChanged += HandleUpgradesChanged;
        timeSystem = new TimeSystem(resourceSystem);
        CacheOfflineProgress(CalculateOfflineProgress());
    }

    private void Start()
    {
        if (uiManager != null)
        {
            uiManager.InitializeUpgradeList(
                upgradeManager.UpgradeStates,
                upgradeManager.TemporaryBoostStates,
                HandleUpgradeBuyRequested,
                HandleTemporaryBoostRequested);
        }

        RefreshUI();
        ShowOfflineRewardIfNeeded();

        if (pendingOfflineOre <= 0 &&
            (pendingOfflineWarehouseOre > 0 ||
             pendingOfflineShuttleOre != gameData.shuttleOre ||
             !Mathf.Approximately(pendingOfflineShuttleCooldown, gameData.shuttleSendCooldownRemaining)))
        {
            ApplyPendingOfflineProgress();
            RefreshUI();
            SaveGame();
        }
    }

    private void Update()
    {
        if (uiManager != null && uiManager.IsOfflineRewardVisible)
        {
            return;
        }

        bool shouldRefreshUi = false;
        bool shouldSaveGame = false;

        if (timeSystem.UpdateTimer(Time.deltaTime))
        {
            shouldRefreshUi = true;
        }

        if (upgradeManager != null)
        {
            upgradeManager.Update(Time.deltaTime);
            UpdateBoostUI();
        }

        if (shuttleSystem != null && shuttleSystem.UpdateCooldown(Time.deltaTime))
        {
            shouldRefreshUi = true;
        }

        if (TryAutoSendShuttle())
        {
            shouldRefreshUi = true;
            shouldSaveGame = true;
        }

        if (shouldRefreshUi)
        {
            RefreshUI();
        }

        autoSaveTimer += Time.deltaTime;

        if (autoSaveTimer >= GameSettings.AutoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveGame();
        }
        else if (shouldSaveGame)
        {
            SaveGame();
        }
    }

    public void OnMineButtonClicked()
    {
        resourceSystem.Mine();
        TryAutoSendShuttle();
        RefreshUI();
    }

    public void OnUpgradeButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideMenu();
        uiManager.HideResetConfirmation();
        uiManager.ToggleUpgradePanel();
    }

    public void OnSendShuttleButtonClicked()
    {
        if (shuttleSystem == null)
        {
            return;
        }

        if (shuttleSystem.SendToWarehouse() <= 0)
        {
            return;
        }

        RefreshUI();
        SaveGame();
    }

    public void OnMenuButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideUpgradePanel();
        uiManager.HideResetConfirmation();
        uiManager.ToggleMenu();
    }

    public void OnResetProgressButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideMenu();
        uiManager.ShowResetConfirmation();
    }

    public void OnConfirmResetButtonClicked()
    {
        ResetGame();
        SaveGame();
        RefreshUI();

        if (uiManager == null)
        {
            return;
        }

        uiManager.HideResetConfirmation();
        uiManager.HideMenu();
    }

    public void OnCancelResetButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideResetConfirmation();
    }

    public void OnClaimOfflineRewardButtonClicked()
    {
        ApplyPendingOfflineProgress();

        TryAutoSendShuttle();

        if (uiManager != null)
        {
            uiManager.HideOfflineReward();
        }

        RefreshUI();
        SaveGame();
    }

    public void OnClaimOfflineRewardX2ButtonClicked()
    {
        // TODO: Show rewarded ad and give x2 offline reward after ad is completed.
        Debug.Log("Rewarded ad is not implemented yet.");
    }

    public void SaveGame()
    {
        if (gameData == null)
        {
            return;
        }

        PlayerPrefs.SetInt(OreKey, gameData.ore);
        PlayerPrefs.SetInt(ShuttleOreKey, gameData.shuttleOre);
        PlayerPrefs.SetInt(ShuttleCapacityKey, gameData.shuttleCapacity);
        PlayerPrefs.SetFloat(ShuttleSendCooldownKey, gameData.shuttleSendCooldownRemaining);
        PlayerPrefs.SetInt(OrePerClickKey, gameData.orePerClick);
        PlayerPrefs.SetInt(OrePerSecondKey, gameData.orePerSecond);
        PlayerPrefs.SetInt(TotalOreEarnedKey, gameData.totalOreEarned);
        PlayerPrefs.DeleteKey(LegacyUpgradeLevelKey);

        if (upgradeManager != null)
        {
            upgradeManager.SaveUpgradeLevels();
        }

        PlayerPrefs.SetString(LastSaveTimeKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        long offlineSeconds = GetOfflineSeconds();
        int loadedShuttleCapacity = Mathf.Max(
            GetConfiguredShuttleCapacity(),
            PlayerPrefs.GetInt(ShuttleCapacityKey, GetConfiguredShuttleCapacity()));
        int loadedShuttleOre = Mathf.Max(
            0,
            PlayerPrefs.GetInt(ShuttleOreKey, GetConfiguredStartShuttleOre()));
        float savedShuttleCooldown = PlayerPrefs.HasKey(ShuttleSendCooldownKey)
            ? PlayerPrefs.GetFloat(ShuttleSendCooldownKey, 0f)
            : 0f;
        float loadedShuttleCooldown = Mathf.Max(
            0f,
            savedShuttleCooldown - offlineSeconds);

        gameData = new GameData
        {
            ore = PlayerPrefs.GetInt(OreKey, GameSettings.StartOre),
            shuttleOre = loadedShuttleOre,
            shuttleCapacity = loadedShuttleCapacity,
            shuttleTravelTimeSeconds = GetConfiguredShuttleTravelTimeSeconds(),
            shuttleSendCooldownRemaining = loadedShuttleCooldown,
            orePerClick = PlayerPrefs.GetInt(OrePerClickKey, GameSettings.StartOrePerClick),
            orePerSecond = PlayerPrefs.GetInt(OrePerSecondKey, GameSettings.StartOrePerSecond),
            totalOreEarned = Mathf.Max(0, PlayerPrefs.GetInt(TotalOreEarnedKey, 0))
        };
    }

    public void ResetGame()
    {
        pendingOfflineOre = 0;
        pendingOfflineWarehouseOre = 0;
        pendingOfflineShuttleOre = 0;
        pendingOfflineShuttleCooldown = 0f;

        if (gameData == null)
        {
            gameData = new GameData();
        }

        gameData.ore = GameSettings.StartOre;
        gameData.shuttleOre = GetConfiguredStartShuttleOre();
        gameData.shuttleCapacity = GetConfiguredShuttleCapacity();
        gameData.shuttleTravelTimeSeconds = GetConfiguredShuttleTravelTimeSeconds();
        gameData.shuttleSendCooldownRemaining = 0f;
        gameData.shuttleAutoSendEnabled = false;
        gameData.orePerClick = GameSettings.StartOrePerClick;
        gameData.orePerSecond = GameSettings.StartOrePerSecond;
        gameData.totalOreEarned = 0;

        PlayerPrefs.DeleteKey(OreKey);
        PlayerPrefs.DeleteKey(ShuttleOreKey);
        PlayerPrefs.DeleteKey(ShuttleCapacityKey);
        PlayerPrefs.DeleteKey(ShuttleSendCooldownKey);
        PlayerPrefs.DeleteKey(OrePerClickKey);
        PlayerPrefs.DeleteKey(OrePerSecondKey);
        PlayerPrefs.DeleteKey(TotalOreEarnedKey);
        PlayerPrefs.DeleteKey(LegacyUpgradeLevelKey);
        PlayerPrefs.DeleteKey(LastSaveTimeKey);

        if (upgradeManager != null)
        {
            upgradeManager.ResetUpgrades();
        }

        if (uiManager != null)
        {
            uiManager.HideOfflineReward();
            uiManager.HideUpgradePanel();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void RefreshUI()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.UpdateUI(gameData);
        uiManager.RefreshUpgradeList(gameData.ore, upgradeManager.ActiveTemporaryBoostStates.Count);
        uiManager.SetMainScreenUpgradeButtonVisible(upgradeManager.HasAffordableUpgrade(gameData.ore));
        UpdateBoostUI();
    }

    private OfflineProgress CalculateOfflineProgress()
    {
        long offlineSeconds = GetOfflineSeconds();

        if (offlineSeconds <= 0)
        {
            return new OfflineProgress
            {
                finalShuttleOre = gameData.shuttleOre,
                finalShuttleCooldown = gameData.shuttleSendCooldownRemaining
            };
        }

        offlineSeconds = Math.Min(offlineSeconds, GameSettings.MaxOfflineSeconds);

        int orePerSecond = Mathf.Max(0, gameData.orePerSecond);
        int shuttleCapacity = Mathf.Max(1, gameData.shuttleCapacity);
        int shuttleOre = Mathf.Max(0, gameData.shuttleOre);
        float shuttleTravelTime = Mathf.Max(0f, gameData.shuttleTravelTimeSeconds);
        float shuttleCooldown = PlayerPrefs.HasKey(ShuttleSendCooldownKey)
            ? PlayerPrefs.GetFloat(ShuttleSendCooldownKey, 0f)
            : 0f;
        bool autoSendEnabled = gameData.shuttleAutoSendEnabled;
        int rewardOre = 0;
        int warehouseOre = 0;
        long remainingSeconds = offlineSeconds;

        if (autoSendEnabled && shuttleCooldown <= 0f && shuttleOre >= shuttleCapacity)
        {
            warehouseOre += shuttleOre;
            shuttleOre = 0;
            shuttleCooldown = shuttleTravelTime;
        }

        if (!autoSendEnabled)
        {
            if (shuttleCooldown > 0f)
            {
                long travelSeconds = Math.Min(remainingSeconds, (long)Math.Ceiling(shuttleCooldown));
                shuttleCooldown = Mathf.Max(0f, shuttleCooldown - travelSeconds);
                remainingSeconds -= travelSeconds;
            }

            if (remainingSeconds > 0 && orePerSecond > 0)
            {
                int freeCapacity = Mathf.Max(0, shuttleCapacity - shuttleOre);
                int minedOre = (int)Math.Min((long)freeCapacity, remainingSeconds * orePerSecond);
                shuttleOre += minedOre;
                rewardOre += minedOre;
            }
        }
        else
        {
            while (remainingSeconds > 0)
            {
                if (shuttleCooldown > 0f)
                {
                    long travelSeconds = Math.Min(remainingSeconds, (long)Math.Ceiling(shuttleCooldown));
                    shuttleCooldown = Mathf.Max(0f, shuttleCooldown - travelSeconds);
                    remainingSeconds -= travelSeconds;
                    continue;
                }

                int freeCapacity = Mathf.Max(0, shuttleCapacity - shuttleOre);

                if (freeCapacity <= 0)
                {
                    warehouseOre += shuttleOre;
                    shuttleOre = 0;
                    shuttleCooldown = shuttleTravelTime;
                    continue;
                }

                if (orePerSecond <= 0)
                {
                    break;
                }

                long secondsToFill = Math.Max(1L, (long)Math.Ceiling((double)freeCapacity / orePerSecond));
                long miningSeconds = Math.Min(remainingSeconds, secondsToFill);
                int minedOre = (int)Math.Min((long)freeCapacity, miningSeconds * orePerSecond);
                shuttleOre += minedOre;
                rewardOre += minedOre;
                remainingSeconds -= miningSeconds;

                if (shuttleOre >= shuttleCapacity)
                {
                    warehouseOre += shuttleOre;
                    shuttleOre = 0;
                    shuttleCooldown = shuttleTravelTime;
                }
            }
        }

        return new OfflineProgress
        {
            rewardOre = rewardOre,
            warehouseOre = warehouseOre,
            finalShuttleOre = shuttleOre,
            finalShuttleCooldown = shuttleCooldown
        };
    }

    private long GetOfflineSeconds()
    {
        string savedTime = PlayerPrefs.GetString(LastSaveTimeKey, string.Empty);

        if (string.IsNullOrEmpty(savedTime))
        {
            return 0;
        }

        if (!long.TryParse(savedTime, out long lastSaveTime))
        {
            return 0;
        }

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Math.Max(0, currentTime - lastSaveTime);
    }

    private int GetConfiguredStartShuttleOre()
    {
        return shuttleConfig != null
            ? shuttleConfig.StartOre
            : ShuttleConfig.DefaultStartOre;
    }

    private int GetConfiguredShuttleCapacity()
    {
        return shuttleConfig != null
            ? shuttleConfig.Capacity
            : ShuttleConfig.DefaultCapacity;
    }

    private float GetConfiguredShuttleTravelTimeSeconds()
    {
        return shuttleConfig != null
            ? shuttleConfig.TravelTimeSeconds
            : ShuttleConfig.DefaultTravelTimeSeconds;
    }

    private void ShowOfflineRewardIfNeeded()
    {
        if (pendingOfflineOre <= 0 || uiManager == null)
        {
            return;
        }

        uiManager.ShowOfflineReward(pendingOfflineOre);
    }

    private void HandleUpgradeBuyRequested(UpgradeState state)
    {
        if (upgradeManager == null || !upgradeManager.TryBuyUpgrade(state))
        {
            return;
        }

        TryAutoSendShuttle();
        RefreshUI();
        SaveGame();
    }

    private void HandleTemporaryBoostRequested(TemporaryBoostState state)
    {
        if (upgradeManager == null || !upgradeManager.TryActivateTemporaryBoost(state))
        {
            return;
        }

        RefreshUI();
        SaveGame();
    }

    private void HandleUpgradesChanged()
    {
        TryAutoSendShuttle();
        RefreshUI();
    }

    private void UpdateBoostUI()
    {
        if (uiManager == null || upgradeManager == null)
        {
            return;
        }

        uiManager.UpdateBoostUI(upgradeManager.ActiveTemporaryBoostStates);
    }

    private void OnDestroy()
    {
        if (upgradeManager != null)
        {
            upgradeManager.UpgradesChanged -= HandleUpgradesChanged;
        }
    }

    private bool TryAutoSendShuttle()
    {
        if (shuttleSystem == null || !gameData.shuttleAutoSendEnabled)
        {
            return false;
        }

        if (!shuttleSystem.IsFull() || !shuttleSystem.CanSend())
        {
            return false;
        }

        return shuttleSystem.SendToWarehouse() > 0;
    }

    private void CacheOfflineProgress(OfflineProgress offlineProgress)
    {
        pendingOfflineOre = offlineProgress.rewardOre;
        pendingOfflineWarehouseOre = offlineProgress.warehouseOre;
        pendingOfflineShuttleOre = offlineProgress.finalShuttleOre;
        pendingOfflineShuttleCooldown = offlineProgress.finalShuttleCooldown;
    }

    private void ApplyPendingOfflineProgress()
    {
        if (pendingOfflineOre <= 0 &&
            pendingOfflineWarehouseOre <= 0 &&
            pendingOfflineShuttleOre == gameData.shuttleOre &&
            Mathf.Approximately(pendingOfflineShuttleCooldown, gameData.shuttleSendCooldownRemaining))
        {
            return;
        }

        gameData.ore += pendingOfflineWarehouseOre;
        gameData.shuttleOre = Mathf.Max(0, pendingOfflineShuttleOre);
        gameData.shuttleSendCooldownRemaining = Mathf.Max(0f, pendingOfflineShuttleCooldown);
        gameData.totalOreEarned += pendingOfflineOre;

        pendingOfflineOre = 0;
        pendingOfflineWarehouseOre = 0;
        pendingOfflineShuttleOre = gameData.shuttleOre;
        pendingOfflineShuttleCooldown = gameData.shuttleSendCooldownRemaining;
    }

    private struct OfflineProgress
    {
        public int rewardOre;
        public int warehouseOre;
        public int finalShuttleOre;
        public float finalShuttleCooldown;
    }
}
