using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string OreKey = "ore";
    private const string ShuttleOreKey = "shuttleOre";
    private const string ShuttleDeliveringOreKey = "shuttleDeliveringOre";
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
    private int pendingOfflineMinedOre;
    private int pendingOfflineWarehouseOre;
    private int pendingOfflineShuttleOre;
    private int pendingOfflineShuttleDeliveringOre;
    private float pendingOfflineShuttleCooldown;
    private bool pendingOfflineShowRewardPopup;

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

        if (!ShouldShowOfflineRewardPopup() && HasPendingOfflineStateChanges())
        {
            ApplyPendingOfflineProgress();
            SaveGame();
        }

        RefreshUI();
        ShowOfflineRewardIfNeeded();
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

    private void SaveGame()
    {
        if (gameData == null)
        {
            return;
        }

        PlayerPrefs.SetInt(OreKey, gameData.ore);
        PlayerPrefs.SetInt(ShuttleOreKey, gameData.shuttleOre);
        PlayerPrefs.SetInt(ShuttleDeliveringOreKey, gameData.shuttleDeliveringOre);
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

    private void LoadGame()
    {
        int loadedShuttleCapacity = Mathf.Max(
            GetConfiguredShuttleCapacity(),
            PlayerPrefs.GetInt(ShuttleCapacityKey, GetConfiguredShuttleCapacity()));
        int loadedShuttleOre = Mathf.Max(
            0,
            PlayerPrefs.GetInt(ShuttleOreKey, GetConfiguredStartShuttleOre()));
        int loadedShuttleDeliveringOre = Mathf.Max(
            0,
            PlayerPrefs.GetInt(ShuttleDeliveringOreKey, 0));
        float savedShuttleCooldown = PlayerPrefs.HasKey(ShuttleSendCooldownKey)
            ? PlayerPrefs.GetFloat(ShuttleSendCooldownKey, 0f)
            : 0f;

        gameData = new GameData
        {
            ore = PlayerPrefs.GetInt(OreKey, GameSettings.StartOre),
            shuttleOre = loadedShuttleOre,
            shuttleDeliveringOre = loadedShuttleDeliveringOre,
            shuttleCapacity = loadedShuttleCapacity,
            shuttleTravelTimeSeconds = GetConfiguredShuttleTravelTimeSeconds(),
            shuttleSendCooldownRemaining = Mathf.Max(0f, savedShuttleCooldown),
            orePerClick = PlayerPrefs.GetInt(OrePerClickKey, GameSettings.StartOrePerClick),
            orePerSecond = PlayerPrefs.GetInt(OrePerSecondKey, GameSettings.StartOrePerSecond),
            totalOreEarned = Mathf.Max(0, PlayerPrefs.GetInt(TotalOreEarnedKey, 0))
        };
    }

    private void ResetGame()
    {
        pendingOfflineMinedOre = 0;
        pendingOfflineWarehouseOre = 0;
        pendingOfflineShuttleOre = 0;
        pendingOfflineShuttleDeliveringOre = 0;
        pendingOfflineShuttleCooldown = 0f;
        pendingOfflineShowRewardPopup = false;

        if (gameData == null)
        {
            gameData = new GameData();
        }

        gameData.ore = GameSettings.StartOre;
        gameData.shuttleOre = GetConfiguredStartShuttleOre();
        gameData.shuttleDeliveringOre = 0;
        gameData.shuttleCapacity = GetConfiguredShuttleCapacity();
        gameData.shuttleTravelTimeSeconds = GetConfiguredShuttleTravelTimeSeconds();
        gameData.shuttleSendCooldownRemaining = 0f;
        gameData.shuttleAutoSendEnabled = false;
        gameData.orePerClick = GameSettings.StartOrePerClick;
        gameData.orePerSecond = GameSettings.StartOrePerSecond;
        gameData.totalOreEarned = 0;

        PlayerPrefs.DeleteKey(OreKey);
        PlayerPrefs.DeleteKey(ShuttleOreKey);
        PlayerPrefs.DeleteKey(ShuttleDeliveringOreKey);
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

        uiManager.UpdateUI(GetDisplayGameData());
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
                minedOre = 0,
                warehouseOre = 0,
                finalShuttleOre = gameData.shuttleOre,
                finalShuttleDeliveringOre = gameData.shuttleDeliveringOre,
                finalShuttleCooldown = gameData.shuttleSendCooldownRemaining
            };
        }

        offlineSeconds = Math.Min(offlineSeconds, GameSettings.MaxOfflineSeconds);

        int orePerSecond = Mathf.Max(0, gameData.orePerSecond);
        int shuttleCapacity = Mathf.Max(1, gameData.shuttleCapacity);
        int shuttleOre = Mathf.Max(0, gameData.shuttleOre);
        int shuttleDeliveringOre = Mathf.Max(
            0,
            PlayerPrefs.GetInt(ShuttleDeliveringOreKey, gameData.shuttleDeliveringOre));
        float shuttleTravelTime = Mathf.Max(0f, gameData.shuttleTravelTimeSeconds);
        float shuttleCooldown = PlayerPrefs.HasKey(ShuttleSendCooldownKey)
            ? PlayerPrefs.GetFloat(ShuttleSendCooldownKey, 0f)
            : 0f;
        bool autoSendEnabled = gameData.shuttleAutoSendEnabled;
        int minedOre = 0;
        int warehouseOre = 0;
        long remainingSeconds = offlineSeconds;

        while (remainingSeconds > 0)
        {
            if (shuttleCooldown > 0f)
            {
                long travelSeconds = Math.Min(remainingSeconds, (long)Math.Ceiling(shuttleCooldown));
                shuttleCooldown = Mathf.Max(0f, shuttleCooldown - travelSeconds);
                remainingSeconds -= travelSeconds;

                if (shuttleCooldown <= 0f && shuttleDeliveringOre > 0)
                {
                    warehouseOre += shuttleDeliveringOre;
                    shuttleDeliveringOre = 0;
                }

                continue;
            }

            if (autoSendEnabled && shuttleOre >= shuttleCapacity)
            {
                if (shuttleTravelTime <= 0f)
                {
                    warehouseOre += shuttleOre;
                    shuttleOre = 0;
                    continue;
                }

                shuttleDeliveringOre = shuttleOre;
                shuttleOre = 0;
                shuttleCooldown = shuttleTravelTime;
                continue;
            }

            if (orePerSecond <= 0)
            {
                break;
            }

            int freeCapacity = Mathf.Max(0, shuttleCapacity - shuttleOre);

            if (freeCapacity <= 0)
            {
                break;
            }

            if (!autoSendEnabled)
            {
                int minedAmount = (int)Math.Min((long)freeCapacity, remainingSeconds * orePerSecond);
                shuttleOre += minedAmount;
                minedOre += minedAmount;
                break;
            }

            long secondsToFill = Math.Max(1L, (long)Math.Ceiling((double)freeCapacity / orePerSecond));
            long miningSeconds = Math.Min(remainingSeconds, secondsToFill);
            int minedAmountToShuttle = (int)Math.Min((long)freeCapacity, miningSeconds * orePerSecond);
            shuttleOre += minedAmountToShuttle;
            minedOre += minedAmountToShuttle;
            remainingSeconds -= miningSeconds;

            if (shuttleOre >= shuttleCapacity)
            {
                if (shuttleTravelTime <= 0f)
                {
                    warehouseOre += shuttleOre;
                    shuttleOre = 0;
                }
                else
                {
                    shuttleDeliveringOre = shuttleOre;
                    shuttleOre = 0;
                    shuttleCooldown = shuttleTravelTime;
                }
            }
        }

        return new OfflineProgress
        {
            minedOre = minedOre,
            warehouseOre = warehouseOre,
            finalShuttleOre = shuttleOre,
            finalShuttleDeliveringOre = shuttleDeliveringOre,
            finalShuttleCooldown = shuttleCooldown,
            shouldShowPopup = autoSendEnabled && warehouseOre > 0
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
        if (!ShouldShowOfflineRewardPopup() || uiManager == null)
        {
            return;
        }

        uiManager.ShowOfflineReward(pendingOfflineWarehouseOre);
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
        pendingOfflineMinedOre = offlineProgress.minedOre;
        pendingOfflineWarehouseOre = offlineProgress.warehouseOre;
        pendingOfflineShuttleOre = offlineProgress.finalShuttleOre;
        pendingOfflineShuttleDeliveringOre = offlineProgress.finalShuttleDeliveringOre;
        pendingOfflineShuttleCooldown = offlineProgress.finalShuttleCooldown;
        pendingOfflineShowRewardPopup = offlineProgress.shouldShowPopup;
    }

    private void ApplyPendingOfflineProgress()
    {
        if (!ShouldShowOfflineRewardPopup() && !HasPendingOfflineStateChanges())
        {
            return;
        }

        gameData.ore += pendingOfflineWarehouseOre;
        gameData.shuttleOre = Mathf.Max(0, pendingOfflineShuttleOre);
        gameData.shuttleDeliveringOre = Mathf.Max(0, pendingOfflineShuttleDeliveringOre);
        gameData.shuttleSendCooldownRemaining = Mathf.Max(0f, pendingOfflineShuttleCooldown);
        gameData.totalOreEarned += pendingOfflineMinedOre;

        pendingOfflineMinedOre = 0;
        pendingOfflineWarehouseOre = 0;
        pendingOfflineShuttleOre = gameData.shuttleOre;
        pendingOfflineShuttleDeliveringOre = gameData.shuttleDeliveringOre;
        pendingOfflineShuttleCooldown = gameData.shuttleSendCooldownRemaining;
        pendingOfflineShowRewardPopup = false;
    }

    private bool ShouldShowOfflineRewardPopup()
    {
        return pendingOfflineShowRewardPopup;
    }

    private GameData GetDisplayGameData()
    {
        if (!ShouldShowOfflineRewardPopup())
        {
            return gameData;
        }

        return new GameData
        {
            ore = gameData.ore,
            shuttleOre = Mathf.Max(0, pendingOfflineShuttleOre),
            shuttleDeliveringOre = Mathf.Max(0, pendingOfflineShuttleDeliveringOre),
            shuttleCapacity = gameData.shuttleCapacity,
            shuttleTravelTimeSeconds = gameData.shuttleTravelTimeSeconds,
            shuttleSendCooldownRemaining = Mathf.Max(0f, pendingOfflineShuttleCooldown),
            shuttleAutoSendEnabled = gameData.shuttleAutoSendEnabled,
            orePerClick = gameData.orePerClick,
            orePerSecond = gameData.orePerSecond,
            totalOreEarned = gameData.totalOreEarned + pendingOfflineMinedOre
        };
    }

    private bool HasPendingOfflineStateChanges()
    {
        return pendingOfflineWarehouseOre > 0 ||
               pendingOfflineShuttleOre != gameData.shuttleOre ||
               pendingOfflineShuttleDeliveringOre != gameData.shuttleDeliveringOre ||
               !Mathf.Approximately(pendingOfflineShuttleCooldown, gameData.shuttleSendCooldownRemaining);
    }

    private struct OfflineProgress
    {
        public int minedOre;
        public int warehouseOre;
        public int finalShuttleOre;
        public int finalShuttleDeliveringOre;
        public float finalShuttleCooldown;
        public bool shouldShowPopup;
    }
}
