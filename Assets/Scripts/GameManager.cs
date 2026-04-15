using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private const string OreKey = "ore";
    private const string ResourceKeyPrefix = "resource_";
    private const string ShuttleOreKey = "shuttleOre";
    private const string ShuttleDockedOreKey = "shuttleDockedOre";
    private const string ShuttleLoadingOreKey = "shuttleLoadingOre";
    private const string ShuttleLoadingTargetOreKey = "shuttleLoadingTargetOre";
    private const string ShuttleLoadingCooldownKey = "shuttleLoadingCooldown";
    private const string ShuttleSendAfterLoadingKey = "shuttleSendAfterLoading";
    private const string ShuttleDeliveringOreKey = "shuttleDeliveringOre";
    private const string PlatformCapacityKey = "platformCapacity";
    private const string ShuttleCapacityKey = "shuttleCapacity";
    private const string ShuttleSendCooldownKey = "shuttleSendCooldown";
    private const string OrePerClickKey = "orePerClick";
    private const string OrePerSecondKey = "orePerSecond";
    private const string EnergyRegenTimerKey = "energyRegenTimer";
    private const string TotalOreEarnedKey = "totalOreEarned";
    private const string LastSaveTimeKey = "lastSaveTime";
    private const string LegacyUpgradeLevelKey = "upgradeLevel";

    [SerializeField] private UIManager uiManager;
    [FormerlySerializedAs("shuttleConfig")]
    [SerializeField] private ShuttleConfig gameConfig;
    [SerializeField] private UpgradeConfig upgradeConfig;
    [SerializeField] private BuildingConfig buildingConfig;
    [SerializeField] private TemporaryBoostConfig temporaryBoostConfig;
    [SerializeField] private MissionConfig missionConfig;
    [SerializeField] private MetaBonusConfig metaBonusConfig;

    private GameData gameData;
    private PlatformSystem platformSystem;
    private ShuttleSystem shuttleSystem;
    private ResourceSystem resourceSystem;
    private UpgradeManager upgradeManager;
    private MissionManager missionManager;
    private TimeSystem timeSystem;
    private EnergySystem energySystem;
    private float autoSaveTimer;
    private int pendingOfflineWarehouseOre;
    private bool pendingOfflineShowRewardPopup;
    private GameData pendingOfflinePreviewData;
    private TemporaryBoostState pendingBoostOfferState;
    private bool suppressBoostOfferPopup;
    private float boostOfferAutoCloseTimer;

    private void Awake()
    {
        LoadGame();

        MissionConfig resolvedMissionConfig = missionConfig != null
            ? missionConfig
            : MissionConfig.CreateRuntimeDefault();
        MetaBonusConfig resolvedMetaBonusConfig = metaBonusConfig != null
            ? metaBonusConfig
            : MetaBonusConfig.CreateRuntimeDefault();

        platformSystem = new PlatformSystem(gameData);
        shuttleSystem = new ShuttleSystem(gameData, platformSystem);
        resourceSystem = new ResourceSystem(gameData, platformSystem);
        missionManager = new MissionManager(gameData, resolvedMissionConfig, resolvedMetaBonusConfig);
        missionManager.LoadProgress();
        upgradeManager = new UpgradeManager(gameData, resourceSystem, gameConfig, upgradeConfig, buildingConfig, temporaryBoostConfig, missionManager);
        upgradeManager.LoadUpgradeLevels();
        upgradeManager.UpgradesChanged += HandleUpgradesChanged;
        timeSystem = new TimeSystem(resourceSystem);
        energySystem = new EnergySystem(gameData, resourceSystem);
        SyncMissionProgress();
        CacheOfflineProgress(CalculateOfflineProgress());
    }

    private void Start()
    {
        if (uiManager != null)
        {
            uiManager.InitializeUpgradeList(
                upgradeManager.UpgradeStates,
                HandleUpgradeBuyRequested);
            uiManager.InitializeBuildingList(
                upgradeManager.BuildingStates,
                HandleBuildingBuyRequested);
            uiManager.InitializeMetaBonusList(
                missionManager != null ? missionManager.MetaBonusStates : null,
                HandleMetaBonusBuyRequested);
        }

        if (!ShouldShowOfflineRewardPopup() && HasPendingOfflineStateChanges())
        {
            ApplyPendingOfflineProgress();
            SyncMissionProgress();
            SaveGame();
        }

        RefreshUI();
        ShowOfflineRewardIfNeeded();
        TryShowNextBoostOffer();
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

        if (energySystem != null && energySystem.Update(Time.deltaTime))
        {
            shouldRefreshUi = true;
        }

        if (upgradeManager != null)
        {
            upgradeManager.Update(Time.deltaTime);
            UpdateBoostUI();
        }

        if (shuttleSystem != null && shuttleSystem.Update(Time.deltaTime))
        {
            shouldRefreshUi = true;
        }

        if (TryAutoSendShuttle())
        {
            shouldRefreshUi = true;
            shouldSaveGame = true;
        }

        if (SyncMissionProgress())
        {
            shouldRefreshUi = true;
            shouldSaveGame = true;
        }

        if (UpdateBoostOfferAutoClose(Time.deltaTime))
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

        TryShowNextBoostOffer();
    }

    public void OnMineButtonClicked()
    {
        resourceSystem.MineOre();
        TryAutoSendShuttle();
        bool missionChanged = SyncMissionProgress();
        RefreshUI();

        if (missionChanged)
        {
            SaveGame();
        }
    }

    public void OnProduceMetalButtonClicked()
    {
        if (resourceSystem == null || !resourceSystem.TryProduceMetal())
        {
            return;
        }

        SyncMissionProgress();
        RefreshUI();
        SaveGame();
    }

    public void OnUpgradeButtonClicked()
    {
        if (uiManager == null || upgradeManager == null || !upgradeManager.HasAnyUnlockedUpgradeCategory())
        {
            return;
        }

        uiManager.HideMenu();
        uiManager.HideResetConfirmation();
        uiManager.OpenUpgradePanel();
    }

    public void OnBuildButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideMenu();
        uiManager.HideResetConfirmation();
        uiManager.OpenBuildingPanel();
    }

    public void OnMissionButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideMenu();
        uiManager.HideResetConfirmation();
        uiManager.OpenMissionPanel();
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
        uiManager.HideBuildPanel();
        uiManager.HideMissionPanel();
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
        SyncMissionProgress();

        if (uiManager != null)
        {
            uiManager.HideOfflineReward();
        }

        RefreshUI();
        SaveGame();
        TryShowNextBoostOffer();
    }

    public void OnUpgradeCategoryTabClicked(int categoryIndex)
    {
        if (uiManager == null || !Enum.IsDefined(typeof(UpgradeCategory), categoryIndex))
        {
            return;
        }

        UpgradeCategory category = (UpgradeCategory)categoryIndex;

        if (upgradeManager == null || !upgradeManager.IsUpgradeCategoryUnlocked(category))
        {
            return;
        }

        uiManager.SetUpgradeCategory(category);
        RefreshUI();
    }

    public void OnClaimOfflineRewardX2ButtonClicked()
    {
        // TODO: Show rewarded ad and give x2 offline reward after ad is completed.
        Debug.Log("Rewarded ad is not implemented yet.");
    }

    public void OnAcceptBoostButtonClicked()
    {
        if (upgradeManager == null)
        {
            return;
        }

        TemporaryBoostState boostState = pendingBoostOfferState;
        pendingBoostOfferState = null;
        boostOfferAutoCloseTimer = 0f;

        if (uiManager != null)
        {
            uiManager.HideBoostOffer();
        }

        if (!upgradeManager.TryActivateTemporaryBoost(boostState))
        {
            TryShowNextBoostOffer();
            return;
        }

        RefreshUI();
        SaveGame();
        TryShowNextBoostOffer();
    }

    public void OnBoostOfferOverlayClicked()
    {
        DismissBoostOfferAndRefresh();
    }

    private void SaveGame()
    {
        if (gameData == null)
        {
            return;
        }

        SaveResource(ResourceType.Ore, gameData.ore);
        SaveResource(ResourceType.Energy, gameData.energy);
        SaveResource(ResourceType.Metal, gameData.metal);
        SaveResource(ResourceType.Crystal, gameData.crystal);
        PlayerPrefs.SetInt(OreKey, gameData.ore);
        PlayerPrefs.SetInt(ShuttleOreKey, gameData.shuttleOre);
        PlayerPrefs.SetInt(ShuttleDockedOreKey, gameData.shuttleDockedOre);
        PlayerPrefs.SetInt(ShuttleLoadingOreKey, gameData.shuttleLoadingOre);
        PlayerPrefs.SetInt(ShuttleLoadingTargetOreKey, gameData.shuttleLoadingTargetOre);
        PlayerPrefs.SetFloat(ShuttleLoadingCooldownKey, gameData.shuttleLoadingCooldownRemaining);
        PlayerPrefs.SetInt(ShuttleSendAfterLoadingKey, gameData.shuttleSendAfterLoading ? 1 : 0);
        PlayerPrefs.SetInt(ShuttleDeliveringOreKey, gameData.shuttleDeliveringOre);
        PlayerPrefs.SetInt(PlatformCapacityKey, gameData.platformCapacity);
        PlayerPrefs.SetInt(ShuttleCapacityKey, gameData.shuttleCapacity);
        PlayerPrefs.SetFloat(ShuttleSendCooldownKey, gameData.shuttleSendCooldownRemaining);
        PlayerPrefs.SetInt(OrePerClickKey, gameData.orePerClick);
        PlayerPrefs.SetInt(OrePerSecondKey, gameData.orePerSecond);
        PlayerPrefs.SetFloat(EnergyRegenTimerKey, gameData.energyRegenTimer);
        PlayerPrefs.SetInt(TotalOreEarnedKey, gameData.totalOreEarned);
        PlayerPrefs.DeleteKey(LegacyUpgradeLevelKey);

        if (upgradeManager != null)
        {
            upgradeManager.SaveUpgradeLevels();
        }

        if (missionManager != null)
        {
            missionManager.SaveProgress();
        }

        PlayerPrefs.SetString(LastSaveTimeKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        gameData = new GameData();

        int loadedShuttleCapacity = Mathf.Max(
            GetConfiguredShuttleCapacity(),
            PlayerPrefs.GetInt(ShuttleCapacityKey, GetConfiguredShuttleCapacity()));
        int loadedPlatformCapacity = Mathf.Max(
            GetConfiguredStartPlatformCapacity(),
            PlayerPrefs.GetInt(PlatformCapacityKey, GetConfiguredStartPlatformCapacity()));
        int loadedShuttleOre = Mathf.Max(
            0,
            PlayerPrefs.GetInt(ShuttleOreKey, GetConfiguredStartShuttleOre()));
        int loadedShuttleDockedOre = Mathf.Clamp(
            Mathf.Max(0, PlayerPrefs.GetInt(ShuttleDockedOreKey, 0)),
            0,
            loadedShuttleCapacity);
        int loadedShuttleLoadingOre = Mathf.Max(
            0,
            PlayerPrefs.GetInt(ShuttleLoadingOreKey, 0));
        int loadedShuttleLoadingTargetOre = Mathf.Max(
            0,
            PlayerPrefs.GetInt(ShuttleLoadingTargetOreKey, 0));
        int loadedShuttleDeliveringOre = Mathf.Max(
            0,
            PlayerPrefs.GetInt(ShuttleDeliveringOreKey, 0));
        float savedShuttleLoadingCooldown = PlayerPrefs.HasKey(ShuttleLoadingCooldownKey)
            ? PlayerPrefs.GetFloat(ShuttleLoadingCooldownKey, 0f)
            : 0f;
        float savedShuttleCooldown = PlayerPrefs.HasKey(ShuttleSendCooldownKey)
            ? PlayerPrefs.GetFloat(ShuttleSendCooldownKey, 0f)
            : 0f;

        gameData.ore = GetSavedResourceAmount(ResourceType.Ore, GetConfiguredStartOre(), OreKey);
        gameData.energy = GetSavedResourceAmount(ResourceType.Energy, GetConfiguredStartEnergy());
        gameData.metal = GetSavedResourceAmount(ResourceType.Metal, GetConfiguredStartMetal());
        gameData.crystal = GetSavedResourceAmount(ResourceType.Crystal, GetConfiguredStartCrystal());
        gameData.shuttleOre = loadedShuttleOre;
        gameData.shuttleDockedOre = loadedShuttleDockedOre;
        gameData.shuttleLoadingOre = loadedShuttleLoadingOre;
        gameData.shuttleLoadingTargetOre = Mathf.Max(gameData.shuttleLoadingOre, loadedShuttleLoadingTargetOre);
        gameData.shuttleDeliveringOre = loadedShuttleDeliveringOre;
        gameData.shuttleSendAfterLoading = PlayerPrefs.GetInt(ShuttleSendAfterLoadingKey, 0) == 1;
        gameData.platformCapacity = loadedPlatformCapacity;
        gameData.shuttleCapacity = loadedShuttleCapacity;
        gameData.shuttleLoadingTimeSeconds = GetConfiguredShuttleLoadingTimeSeconds();
        gameData.shuttleLoadingCooldownRemaining = Mathf.Max(0f, savedShuttleLoadingCooldown);
        gameData.shuttleTravelTimeSeconds = GetConfiguredShuttleTravelTimeSeconds();
        gameData.shuttleSendCooldownRemaining = Mathf.Max(0f, savedShuttleCooldown);
        gameData.orePerClick = PlayerPrefs.GetInt(OrePerClickKey, GetConfiguredStartOrePerClick());
        gameData.orePerSecond = PlayerPrefs.GetInt(OrePerSecondKey, GetConfiguredStartOrePerSecond());
        gameData.energyMax = GetConfiguredStartEnergyMax();
        gameData.energyRegenAmount = GetConfiguredStartEnergyRegenAmount();
        gameData.energyRegenInterval = GetConfiguredStartEnergyRegenInterval();
        gameData.energyRegenTimer = PlayerPrefs.GetFloat(EnergyRegenTimerKey, 0f);
        gameData.metalPerCraft = GetConfiguredMetalPerCraft();
        gameData.metalOreCost = GetConfiguredMetalOreCost();
        gameData.metalEnergyCost = GetConfiguredMetalEnergyCost();
        gameData.totalOreEarned = Mathf.Max(0, PlayerPrefs.GetInt(TotalOreEarnedKey, 0));
        gameData.EnsureDefaultResources();
    }

    private void ResetGame()
    {
        pendingOfflineWarehouseOre = 0;
        pendingOfflineShowRewardPopup = false;
        pendingOfflinePreviewData = null;
        pendingBoostOfferState = null;

        if (gameData == null)
        {
            gameData = new GameData();
        }

        gameData.ore = GetConfiguredStartOre();
        gameData.energy = GetConfiguredStartEnergy();
        gameData.metal = GetConfiguredStartMetal();
        gameData.crystal = GetConfiguredStartCrystal();
        gameData.shuttleOre = GetConfiguredStartShuttleOre();
        gameData.shuttleDockedOre = 0;
        gameData.shuttleLoadingOre = 0;
        gameData.shuttleLoadingTargetOre = 0;
        gameData.shuttleSendAfterLoading = false;
        gameData.shuttleDeliveringOre = 0;
        gameData.platformCapacity = GetConfiguredStartPlatformCapacity();
        gameData.shuttleCapacity = GetConfiguredShuttleCapacity();
        gameData.shuttleLoadingTimeSeconds = GetConfiguredShuttleLoadingTimeSeconds();
        gameData.shuttleLoadingCooldownRemaining = 0f;
        gameData.shuttleTravelTimeSeconds = GetConfiguredShuttleTravelTimeSeconds();
        gameData.shuttleSendCooldownRemaining = 0f;
        gameData.shuttleAutoSendEnabled = false;
        gameData.orePerClick = GetConfiguredStartOrePerClick();
        gameData.orePerSecond = GetConfiguredStartOrePerSecond();
        gameData.energyMax = GetConfiguredStartEnergyMax();
        gameData.energyRegenAmount = GetConfiguredStartEnergyRegenAmount();
        gameData.energyRegenInterval = GetConfiguredStartEnergyRegenInterval();
        gameData.energyRegenTimer = 0f;
        gameData.metalPerCraft = GetConfiguredMetalPerCraft();
        gameData.metalOreCost = GetConfiguredMetalOreCost();
        gameData.metalEnergyCost = GetConfiguredMetalEnergyCost();
        gameData.totalOreEarned = 0;
        gameData.EnsureDefaultResources();

        PlayerPrefs.DeleteKey(OreKey);
        DeleteResourceKey(ResourceType.Ore);
        DeleteResourceKey(ResourceType.Energy);
        DeleteResourceKey(ResourceType.Metal);
        DeleteResourceKey(ResourceType.Crystal);
        PlayerPrefs.DeleteKey(ShuttleOreKey);
        PlayerPrefs.DeleteKey(ShuttleDockedOreKey);
        PlayerPrefs.DeleteKey(ShuttleLoadingOreKey);
        PlayerPrefs.DeleteKey(ShuttleLoadingTargetOreKey);
        PlayerPrefs.DeleteKey(ShuttleLoadingCooldownKey);
        PlayerPrefs.DeleteKey(ShuttleSendAfterLoadingKey);
        PlayerPrefs.DeleteKey(ShuttleDeliveringOreKey);
        PlayerPrefs.DeleteKey(PlatformCapacityKey);
        PlayerPrefs.DeleteKey(ShuttleCapacityKey);
        PlayerPrefs.DeleteKey(ShuttleSendCooldownKey);
        PlayerPrefs.DeleteKey(OrePerClickKey);
        PlayerPrefs.DeleteKey(OrePerSecondKey);
        PlayerPrefs.DeleteKey(EnergyRegenTimerKey);
        PlayerPrefs.DeleteKey(TotalOreEarnedKey);
        PlayerPrefs.DeleteKey(LegacyUpgradeLevelKey);
        PlayerPrefs.DeleteKey(LastSaveTimeKey);

        if (upgradeManager != null)
        {
            upgradeManager.ResetUpgrades();
        }

        if (missionManager != null)
        {
            missionManager.ResetProgress();

            if (upgradeManager != null)
            {
                upgradeManager.RecalculateIncome();
            }
        }

        if (uiManager != null)
        {
            uiManager.HideOfflineReward();
            uiManager.HideUpgradePanel();
            uiManager.HideBuildPanel();
            uiManager.HideMissionPanel();
            uiManager.HideBoostOffer();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            suppressBoostOfferPopup = true;
            DismissPendingBoostOffer();
            SaveGame();
            return;
        }

        suppressBoostOfferPopup = false;
        TryShowNextBoostOffer();
    }

    private void OnApplicationQuit()
    {
        suppressBoostOfferPopup = true;
        DismissPendingBoostOffer();
        SaveGame();
    }

    private void RefreshUI()
    {
        if (uiManager == null)
        {
            return;
        }

        GameData displayData = GetDisplayGameData();
        uiManager.UpdateUI(displayData);
        uiManager.UpdateUpgradeCategoryTabs(
            upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.Miner),
            upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.Platform),
            upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.PowerStation),
            upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.Factory),
            upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.Shuttle));
        uiManager.RefreshUpgradeList(displayData);
        uiManager.RefreshBuildingList(displayData);
        uiManager.RefreshMetaBonusList(displayData);
        uiManager.SetMainScreenUpgradeButtonVisible(
            upgradeManager.HasAnyUnlockedUpgradeCategory() &&
            upgradeManager.HasAffordableUpgrade());
        uiManager.SetMainScreenBuildButtonVisible(upgradeManager.BuildingStates.Count > 0);
        uiManager.SetMainScreenMissionButtonVisible(missionManager != null);
        uiManager.UpdateMissionInfo(
            missionManager != null
                ? missionManager.GetMissionProgress(upgradeManager)
                : default);
        UpdateBoostUI();
    }

    private OfflineProgress CalculateOfflineProgress()
    {
        long offlineSeconds = GetOfflineSeconds();
        GameData previewData = gameData.Clone();

        if (offlineSeconds <= 0)
        {
            return new OfflineProgress
            {
                warehouseOre = 0,
                previewData = previewData
            };
        }

        offlineSeconds = Math.Min(offlineSeconds, GameSettings.MaxOfflineSeconds);

        if (energySystem != null)
        {
            EnergySystem.EnergyOfflineProgress energyProgress = energySystem.CalculateOfflineProgress(offlineSeconds);
            previewData.energy = energyProgress.energy;
            previewData.energyRegenTimer = energyProgress.regenTimer;
        }

        int orePerSecond = Mathf.Max(0, previewData.orePerSecond);
        bool hasMiningPlatform = previewData.hasMiningPlatform;
        int platformCapacity = hasMiningPlatform
            ? Mathf.Max(1, previewData.platformCapacity)
            : Mathf.Max(1, previewData.shuttleCapacity);
        int shuttleCapacity = Mathf.Max(1, previewData.shuttleCapacity);
        int storedPlatformOre = Mathf.Max(0, previewData.shuttleOre);
        int shuttleDockedOre = Mathf.Clamp(Mathf.Max(0, previewData.shuttleDockedOre), 0, shuttleCapacity);
        int shuttleLoadingOre = Mathf.Max(0, previewData.shuttleLoadingOre);
        int shuttleLoadingTargetOre = Mathf.Max(shuttleLoadingOre, previewData.shuttleLoadingTargetOre);
        float shuttleLoadingTime = Mathf.Max(0f, previewData.shuttleLoadingTimeSeconds);
        float shuttleLoadingCooldown = Mathf.Max(0f, previewData.shuttleLoadingCooldownRemaining);
        bool shuttleSendAfterLoading = previewData.shuttleSendAfterLoading;
        int shuttleDeliveringOre = Mathf.Max(0, previewData.shuttleDeliveringOre);
        float shuttleTravelTime = Mathf.Max(0f, previewData.shuttleTravelTimeSeconds);
        float shuttleCooldown = Mathf.Max(0f, previewData.shuttleSendCooldownRemaining);
        bool autoSendEnabled = hasMiningPlatform && previewData.shuttleAutoSendEnabled;
        int minedOre = 0;
        int warehouseOre = 0;
        long remainingSeconds = offlineSeconds;

        while (remainingSeconds > 0)
        {
            if (shuttleLoadingCooldown > 0f && shuttleLoadingTargetOre > 0)
            {
                long loadingSeconds = Math.Min(remainingSeconds, (long)Math.Ceiling(shuttleLoadingCooldown));
                shuttleLoadingCooldown = Mathf.Max(0f, shuttleLoadingCooldown - loadingSeconds);
                remainingSeconds -= loadingSeconds;

                int desiredLoadedOre = GetOfflineLoadedOre(
                    shuttleLoadingTargetOre,
                    shuttleLoadingTime,
                    shuttleLoadingCooldown);
                int deltaLoadedOre = Math.Max(0, desiredLoadedOre - shuttleLoadingOre);
                int transferredOre = Math.Min(deltaLoadedOre, storedPlatformOre);
                storedPlatformOre -= transferredOre;
                shuttleLoadingOre += transferredOre;

                if (shuttleLoadingCooldown <= 0f)
                {
                    int remainingLoadOre = Math.Min(
                        Math.Max(0, shuttleLoadingTargetOre - shuttleLoadingOre),
                        storedPlatformOre);
                    storedPlatformOre -= remainingLoadOre;
                    shuttleLoadingOre += remainingLoadOre;
                    shuttleDockedOre = Math.Min(shuttleCapacity, shuttleDockedOre + shuttleLoadingOre);

                    shuttleLoadingOre = 0;
                    shuttleLoadingTargetOre = 0;

                    if (shuttleSendAfterLoading)
                    {
                        int cargoToSend = shuttleDockedOre;
                        shuttleDockedOre = 0;

                        if (shuttleTravelTime <= 0f)
                        {
                            warehouseOre += cargoToSend;
                        }
                        else
                        {
                            shuttleDeliveringOre = cargoToSend;
                            shuttleCooldown = shuttleTravelTime;
                        }
                    }

                    shuttleSendAfterLoading = false;
                }

                continue;
            }

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

            if (autoSendEnabled && shuttleDockedOre >= shuttleCapacity)
            {
                int cargoToSend = shuttleDockedOre;
                shuttleDockedOre = 0;

                if (shuttleTravelTime <= 0f)
                {
                    warehouseOre += cargoToSend;
                }
                else
                {
                    shuttleDeliveringOre = cargoToSend;
                    shuttleCooldown = shuttleTravelTime;
                }

                continue;
            }

            int remainingShuttleCapacity = Math.Max(0, shuttleCapacity - shuttleDockedOre);
            int autoSendThreshold = remainingShuttleCapacity <= 0
                ? 0
                : Math.Max(1, Math.Min(platformCapacity, remainingShuttleCapacity));

            if (autoSendEnabled && autoSendThreshold > 0 && storedPlatformOre >= autoSendThreshold)
            {
                int sentAmount = Math.Min(storedPlatformOre, remainingShuttleCapacity);

                if (shuttleLoadingTime > 0f)
                {
                    shuttleLoadingTargetOre = sentAmount;
                    shuttleLoadingOre = 0;
                    shuttleLoadingCooldown = shuttleLoadingTime;
                    shuttleSendAfterLoading = shuttleDockedOre + sentAmount >= shuttleCapacity;
                    continue;
                }

                storedPlatformOre -= sentAmount;
                shuttleDockedOre = Math.Min(shuttleCapacity, shuttleDockedOre + sentAmount);

                if (shuttleDockedOre >= shuttleCapacity)
                {
                    int cargoToSend = shuttleDockedOre;
                    shuttleDockedOre = 0;

                    if (shuttleTravelTime <= 0f)
                    {
                        warehouseOre += cargoToSend;
                    }
                    else
                    {
                        shuttleDeliveringOre = cargoToSend;
                        shuttleCooldown = shuttleTravelTime;
                    }
                }

                continue;
            }

            if (orePerSecond <= 0)
            {
                break;
            }

            int freeCapacity = Mathf.Max(0, platformCapacity - storedPlatformOre);

            if (freeCapacity <= 0)
            {
                break;
            }

            if (!autoSendEnabled)
            {
                int minedAmount = (int)Math.Min((long)freeCapacity, remainingSeconds * orePerSecond);
                storedPlatformOre += minedAmount;
                minedOre += minedAmount;
                break;
            }

            int neededForAutoSend = Mathf.Max(0, autoSendThreshold - storedPlatformOre);
            long secondsToReady = Math.Max(1L, (long)Math.Ceiling((double)neededForAutoSend / orePerSecond));
            long miningSeconds = Math.Min(remainingSeconds, secondsToReady);
            int minedAmountToShuttle = (int)Math.Min((long)freeCapacity, miningSeconds * orePerSecond);
            storedPlatformOre += minedAmountToShuttle;
            minedOre += minedAmountToShuttle;
            remainingSeconds -= miningSeconds;
        }

        return new OfflineProgress
        {
            warehouseOre = warehouseOre,
            previewData = BuildOfflinePreviewData(
                previewData,
                storedPlatformOre,
                shuttleDockedOre,
                shuttleLoadingOre,
                shuttleLoadingTargetOre,
                shuttleLoadingCooldown,
                shuttleSendAfterLoading,
                shuttleDeliveringOre,
                shuttleCooldown,
                minedOre,
                warehouseOre),
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
        return gameConfig != null
            ? gameConfig.ShuttleStartOre
            : ShuttleConfig.DefaultStartOre;
    }

    private int GetConfiguredStartPlatformCapacity()
    {
        return gameConfig != null
            ? gameConfig.StartPlatformCapacity
            : ShuttleConfig.DefaultPlatformCapacity;
    }

    private int GetConfiguredShuttleCapacity()
    {
        return gameConfig != null
            ? gameConfig.Capacity
            : ShuttleConfig.DefaultCapacity;
    }

    private float GetConfiguredShuttleLoadingTimeSeconds()
    {
        return gameConfig != null
            ? gameConfig.LoadingTimeSeconds
            : ShuttleConfig.DefaultLoadingTimeSeconds;
    }

    private float GetConfiguredShuttleTravelTimeSeconds()
    {
        return gameConfig != null
            ? gameConfig.TravelTimeSeconds
            : ShuttleConfig.DefaultTravelTimeSeconds;
    }

    private int GetConfiguredStartOre()
    {
        return gameConfig != null
            ? gameConfig.StartOre
            : ShuttleConfig.DefaultStartOre;
    }

    private int GetConfiguredStartEnergy()
    {
        return gameConfig != null
            ? gameConfig.StartEnergy
            : ShuttleConfig.DefaultStartEnergy;
    }

    private int GetConfiguredStartMetal()
    {
        return gameConfig != null
            ? gameConfig.StartMetal
            : ShuttleConfig.DefaultStartMetal;
    }

    private int GetConfiguredStartCrystal()
    {
        return gameConfig != null
            ? gameConfig.StartCrystal
            : ShuttleConfig.DefaultStartCrystal;
    }

    private int GetConfiguredStartOrePerClick()
    {
        return gameConfig != null
            ? gameConfig.StartOrePerClick
            : ShuttleConfig.DefaultStartOrePerClick;
    }

    private int GetConfiguredStartOrePerSecond()
    {
        return gameConfig != null
            ? gameConfig.StartOrePerSecond
            : ShuttleConfig.DefaultStartOrePerSecond;
    }

    private int GetConfiguredStartEnergyMax()
    {
        return gameConfig != null
            ? gameConfig.StartEnergyMax
            : ShuttleConfig.DefaultStartEnergyMax;
    }

    private int GetConfiguredStartEnergyRegenAmount()
    {
        return gameConfig != null
            ? gameConfig.StartEnergyRegenAmount
            : ShuttleConfig.DefaultStartEnergyRegenAmount;
    }

    private float GetConfiguredStartEnergyRegenInterval()
    {
        return gameConfig != null
            ? gameConfig.StartEnergyRegenInterval
            : ShuttleConfig.DefaultStartEnergyRegenInterval;
    }

    private int GetConfiguredMetalPerCraft()
    {
        return gameConfig != null
            ? gameConfig.MetalPerCraft
            : ShuttleConfig.DefaultMetalPerCraft;
    }

    private int GetConfiguredMetalOreCost()
    {
        return gameConfig != null
            ? gameConfig.MetalOreCost
            : ShuttleConfig.DefaultMetalOreCost;
    }

    private int GetConfiguredMetalEnergyCost()
    {
        return gameConfig != null
            ? gameConfig.MetalEnergyCost
            : ShuttleConfig.DefaultMetalEnergyCost;
    }

    private float GetConfiguredBoostOfferAutoCloseSeconds()
    {
        return gameConfig != null
            ? gameConfig.BoostOfferAutoCloseSeconds
            : ShuttleConfig.DefaultBoostOfferAutoCloseSeconds;
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
        SyncMissionProgress();
        RefreshUI();
        SaveGame();
    }

    private void HandleBuildingBuyRequested(BuildingState state)
    {
        if (upgradeManager == null || !upgradeManager.TryBuyBuilding(state))
        {
            return;
        }

        TryAutoSendShuttle();
        SyncMissionProgress();
        RefreshUI();
        SaveGame();
    }

    private void HandleMetaBonusBuyRequested(MetaBonusState state)
    {
        if (missionManager == null || !missionManager.TryBuyMetaBonus(state))
        {
            return;
        }

        if (upgradeManager != null)
        {
            upgradeManager.RecalculateIncome();
        }

        RefreshUI();
        SaveGame();
    }

    private void HandleUpgradesChanged()
    {
        TryAutoSendShuttle();
        SyncMissionProgress();
        RefreshUI();
        TryShowNextBoostOffer();
    }

    private void UpdateBoostUI()
    {
        if (uiManager == null || upgradeManager == null)
        {
            return;
        }

        uiManager.UpdateBoostUI(upgradeManager.ActiveTemporaryBoostStates);
    }

    private bool UpdateBoostOfferAutoClose(float deltaTime)
    {
        if (uiManager == null || !uiManager.IsBoostOfferVisible || pendingBoostOfferState == null)
        {
            return false;
        }

        boostOfferAutoCloseTimer -= deltaTime;

        if (boostOfferAutoCloseTimer > 0f)
        {
            return false;
        }

        DismissPendingBoostOffer();
        return true;
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

        if (!shuttleSystem.IsReadyForAutoSend())
        {
            return false;
        }

        return shuttleSystem.AutoSendToWarehouse() > 0;
    }

    private bool SyncMissionProgress()
    {
        return missionManager != null &&
               upgradeManager != null &&
               missionManager.UpdateMissionProgress(upgradeManager);
    }

    private void CacheOfflineProgress(OfflineProgress offlineProgress)
    {
        pendingOfflineWarehouseOre = offlineProgress.warehouseOre;
        pendingOfflinePreviewData = offlineProgress.previewData;
        pendingOfflineShowRewardPopup = offlineProgress.shouldShowPopup;
    }

    private void TryShowNextBoostOffer()
    {
        if (uiManager == null ||
            upgradeManager == null ||
            suppressBoostOfferPopup ||
            uiManager.IsBusyWithOtherWindow ||
            upgradeManager.ActiveTemporaryBoostStates.Count >= GameSettings.MaxActiveTemporaryBoosts)
        {
            return;
        }

        if (pendingBoostOfferState != null &&
            (!pendingBoostOfferState.IsAvailable || pendingBoostOfferState.IsActive))
        {
            pendingBoostOfferState = null;

            if (uiManager.IsBoostOfferVisible)
            {
                uiManager.HideBoostOffer();
            }
        }

        if (uiManager.IsBoostOfferVisible)
        {
            return;
        }

        if (pendingBoostOfferState == null)
        {
            pendingBoostOfferState = upgradeManager.GetNextAvailableTemporaryBoost();
        }

        if (pendingBoostOfferState == null)
        {
            return;
        }

        boostOfferAutoCloseTimer = GetConfiguredBoostOfferAutoCloseSeconds();
        uiManager.ShowBoostOffer(pendingBoostOfferState);
    }

    private void DismissPendingBoostOffer()
    {
        if (uiManager != null)
        {
            uiManager.HideBoostOffer();
        }

        if (upgradeManager == null || pendingBoostOfferState == null)
        {
            pendingBoostOfferState = null;
            return;
        }

        TemporaryBoostState boostState = pendingBoostOfferState;
        pendingBoostOfferState = null;
        boostOfferAutoCloseTimer = 0f;
        upgradeManager.TryDeclineTemporaryBoost(boostState);
    }

    private void DismissBoostOfferAndRefresh()
    {
        if (pendingBoostOfferState == null && (uiManager == null || !uiManager.IsBoostOfferVisible))
        {
            return;
        }

        DismissPendingBoostOffer();
        RefreshUI();
        SaveGame();
        TryShowNextBoostOffer();
    }

    private void ApplyPendingOfflineProgress()
    {
        if (!ShouldShowOfflineRewardPopup() && !HasPendingOfflineStateChanges())
        {
            return;
        }

        if (pendingOfflinePreviewData != null)
        {
            gameData.CopyFrom(pendingOfflinePreviewData);
        }

        pendingOfflineWarehouseOre = 0;
        pendingOfflinePreviewData = null;
        pendingOfflineShowRewardPopup = false;
    }

    private bool ShouldShowOfflineRewardPopup()
    {
        return pendingOfflineShowRewardPopup;
    }

    private GameData GetDisplayGameData()
    {
        if (!ShouldShowOfflineRewardPopup() || pendingOfflinePreviewData == null)
        {
            return gameData;
        }

        return pendingOfflinePreviewData;
    }

    private bool HasPendingOfflineStateChanges()
    {
        if (pendingOfflinePreviewData == null)
        {
            return false;
        }

        return pendingOfflinePreviewData.ore != gameData.ore ||
               pendingOfflinePreviewData.energy != gameData.energy ||
               pendingOfflinePreviewData.metal != gameData.metal ||
               pendingOfflinePreviewData.crystal != gameData.crystal ||
               pendingOfflinePreviewData.shuttleOre != gameData.shuttleOre ||
               pendingOfflinePreviewData.shuttleDockedOre != gameData.shuttleDockedOre ||
               pendingOfflinePreviewData.shuttleLoadingOre != gameData.shuttleLoadingOre ||
               pendingOfflinePreviewData.shuttleLoadingTargetOre != gameData.shuttleLoadingTargetOre ||
               pendingOfflinePreviewData.shuttleSendAfterLoading != gameData.shuttleSendAfterLoading ||
               pendingOfflinePreviewData.shuttleDeliveringOre != gameData.shuttleDeliveringOre ||
               pendingOfflinePreviewData.platformCapacity != gameData.platformCapacity ||
               !Mathf.Approximately(pendingOfflinePreviewData.shuttleLoadingCooldownRemaining, gameData.shuttleLoadingCooldownRemaining) ||
               !Mathf.Approximately(pendingOfflinePreviewData.shuttleSendCooldownRemaining, gameData.shuttleSendCooldownRemaining) ||
               !Mathf.Approximately(pendingOfflinePreviewData.energyRegenTimer, gameData.energyRegenTimer) ||
               pendingOfflinePreviewData.totalOreEarned != gameData.totalOreEarned;
    }

    private GameData BuildOfflinePreviewData(
        GameData previewData,
        int shuttleOre,
        int shuttleDockedOre,
        int shuttleLoadingOre,
        int shuttleLoadingTargetOre,
        float shuttleLoadingCooldown,
        bool shuttleSendAfterLoading,
        int shuttleDeliveringOre,
        float shuttleCooldown,
        int minedOre,
        int warehouseOre)
    {
        previewData.shuttleOre = Mathf.Max(0, shuttleOre);
        previewData.shuttleDockedOre = Mathf.Max(0, shuttleDockedOre);
        previewData.shuttleLoadingOre = Mathf.Max(0, shuttleLoadingOre);
        previewData.shuttleLoadingTargetOre = Mathf.Max(previewData.shuttleLoadingOre, shuttleLoadingTargetOre);
        previewData.shuttleLoadingCooldownRemaining = Mathf.Max(0f, shuttleLoadingCooldown);
        previewData.shuttleSendAfterLoading = shuttleSendAfterLoading;
        previewData.shuttleDeliveringOre = Mathf.Max(0, shuttleDeliveringOre);
        previewData.shuttleSendCooldownRemaining = Mathf.Max(0f, shuttleCooldown);
        previewData.ore += warehouseOre;
        previewData.totalOreEarned += minedOre;
        return previewData;
    }

    private int GetOfflineLoadedOre(int targetOre, float loadingTimeSeconds, float loadingCooldownRemaining)
    {
        if (targetOre <= 0)
        {
            return 0;
        }

        if (loadingTimeSeconds <= 0f || loadingCooldownRemaining <= 0f)
        {
            return targetOre;
        }

        float progress = 1f - Mathf.Clamp01(loadingCooldownRemaining / loadingTimeSeconds);
        return Mathf.Clamp(Mathf.FloorToInt(targetOre * progress), 0, targetOre);
    }

    private void SaveResource(ResourceType resourceType, int amount)
    {
        PlayerPrefs.SetInt(GetResourceKey(resourceType), Mathf.Max(0, amount));
    }

    private int GetSavedResourceAmount(ResourceType resourceType, int defaultValue, string legacyKey = null)
    {
        string resourceKey = GetResourceKey(resourceType);

        if (PlayerPrefs.HasKey(resourceKey))
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(resourceKey, defaultValue));
        }

        if (!string.IsNullOrEmpty(legacyKey))
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(legacyKey, defaultValue));
        }

        return defaultValue;
    }

    private string GetResourceKey(ResourceType resourceType)
    {
        return ResourceKeyPrefix + resourceType;
    }

    private void DeleteResourceKey(ResourceType resourceType)
    {
        PlayerPrefs.DeleteKey(GetResourceKey(resourceType));
    }

    private struct OfflineProgress
    {
        public int warehouseOre;
        public GameData previewData;
        public bool shouldShowPopup;
    }
}
