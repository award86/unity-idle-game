using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private const string OreKey = "ore";
    private const string ResourceKeyPrefix = "resource_";
    private const string ShuttleStateKeyPrefix = "shuttle_state_";
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
    private readonly List<TemporaryBoostState> pendingBoostOfferStates = new List<TemporaryBoostState>();
    private readonly Dictionary<TemporaryBoostState, float> boostOfferAutoCloseTimers = new Dictionary<TemporaryBoostState, float>();
    private bool suppressBoostOfferPopup;
    private bool isApplicationInBackground;

    private void Awake()
    {
        GameTextProvider.Configure(gameConfig);
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
        AdvanceTemporaryBoostTimersForInactiveTime();
        SyncPendingOfflinePreviewWithCurrentDynamicStats();
    }

    private void Start()
    {
        if (uiManager != null)
        {
            uiManager.InitializeShuttleButtons(HandleShuttleSendRequested);
            uiManager.InitializeShuttleDisplays();
            uiManager.InitializeMenuButtons(HandleExitRequested);
            uiManager.InitializeMainScreenActionButtons(OnUpgradeButtonClicked, OnBuildButtonClicked);
            uiManager.InitializeBoostOfferButton(HandleBoostOfferAccepted);
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

        if (!ShouldShowOfflineRewardPopup())
        {
            if (HasPendingOfflineStateChanges())
            {
                ApplyPendingOfflineProgress();
            }

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

        PrepareMainPanelOpen();
        uiManager.SetUpgradeCategory(upgradeManager.GetPreferredAffordableUpgradeCategory());
        uiManager.OpenUpgradePanel();
    }

    public void OnBuildButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        PrepareMainPanelOpen();
        uiManager.OpenBuildingPanel();
    }

    public void OnMissionButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        PrepareMainPanelOpen();
        uiManager.OpenMissionPanel();
    }

    public void OnClaimMissionRewardButtonClicked()
    {
        if (missionManager == null || !missionManager.TryClaimActiveMissionReward())
        {
            return;
        }

        SyncMissionProgress();
        RefreshUI();
        SaveGame();
    }

    public void OnSendShuttleButtonClicked()
    {
        OnSendShuttleButtonClicked(0);
    }

    public void OnSendShuttleButtonClicked(int shuttleIndex)
    {
        if (shuttleSystem == null)
        {
            return;
        }

        if (shuttleSystem.SendToWarehouse(shuttleIndex) <= 0)
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

    public void OnExitGameButtonClicked()
    {
        suppressBoostOfferPopup = true;
        uiManager?.HideBoostOffer();
        SaveGame();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
        // Rewarded ad flow is not implemented yet.
    }

    public void OnAcceptBoostButtonClicked()
    {
        if (pendingBoostOfferStates.Count <= 0)
        {
            return;
        }

        HandleBoostOfferAccepted(pendingBoostOfferStates[0]);
    }

    private void HandleBoostOfferAccepted(TemporaryBoostState boostState)
    {
        if (upgradeManager == null || boostState == null)
        {
            return;
        }

        if (!upgradeManager.TryActivateTemporaryBoost(boostState))
        {
            TryShowNextBoostOffer();
            return;
        }

        RemovePendingBoostOffer(boostState);
        RefreshUI();
        SaveGame();
        TryShowNextBoostOffer();
    }

    public void OnBoostOfferOverlayClicked()
    {
        // Shared overlay is now used by the dropdown menu, not by boost offers.
        // Keep this method as a no-op for old scene bindings so overlay clicks
        // do not accidentally dismiss active boost offer buttons.
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

        for (int i = 0; i < GameData.MaxShuttles; i++)
        {
            SaveShuttleState(i, gameData.GetShuttleState(i));
        }

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
        gameData.EnsureDefaultShuttles();

        for (int i = 0; i < GameData.MaxShuttles; i++)
        {
            LoadShuttleState(i, gameData.GetShuttleState(i));
        }
    }

    private void ResetGame()
    {
        pendingOfflineWarehouseOre = 0;
        pendingOfflineShowRewardPopup = false;
        pendingOfflinePreviewData = null;
        pendingBoostOfferStates.Clear();
        boostOfferAutoCloseTimers.Clear();

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
        gameData.shuttleCount = 1;
        gameData.shuttleAutoSendCount = 0;
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
        gameData.EnsureDefaultShuttles();

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

        for (int i = 0; i < GameData.MaxShuttles; i++)
        {
            DeleteShuttleState(i);
            gameData.GetShuttleState(i).Reset();
        }

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
            HandleApplicationSentToBackground();
            return;
        }

        HandleApplicationReturnedToForeground();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            HandleApplicationSentToBackground();
            return;
        }

        HandleApplicationReturnedToForeground();
    }

    private void OnApplicationQuit()
    {
        suppressBoostOfferPopup = true;
        uiManager?.HideBoostOffer();
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
        uiManager.SetMainScreenBuildButtonVisible(upgradeManager.HasAffordableBuilding());
        uiManager.SetMainScreenMissionButtonVisible(missionManager != null);
        uiManager.UpdateMissionInfo(
            missionManager != null
                ? missionManager.GetMissionProgress(upgradeManager)
                : default,
            GetConfiguredNoMissionsText());
        UpdateBoostUI();
    }

    private OfflineProgress CalculateOfflineProgress()
    {
        long offlineSeconds = GetOfflineSeconds();
        GameData previewData = gameData.Clone();
        List<OfflineBoostSnapshot> offlineBoosts = CreateOfflineBoostSnapshots();
        int baseOrePerSecond = RemoveTemporaryBoostMultiplier(
            previewData.orePerSecond,
            GetOfflineBoostMultiplier(offlineBoosts, TemporaryBoostTargetType.OrePerSecond));
        float baseShuttleTravelTime = RemoveTemporaryTravelSpeedMultiplier(
            previewData.shuttleTravelTimeSeconds,
            GetOfflineBoostMultiplier(offlineBoosts, TemporaryBoostTargetType.ShuttleTravelSpeed));
        bool shouldShowPopupForDuration = offlineSeconds >= GameSettings.MinInactiveSecondsForOffline;

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

        bool hasMiningPlatform = previewData.hasMiningPlatform;
        int platformCapacity = hasMiningPlatform
            ? Mathf.Max(1, previewData.platformCapacity)
            : Mathf.Max(1, previewData.shuttleCapacity);
        int storedPlatformOre = Mathf.Max(0, previewData.shuttleOre);
        int shuttleCapacity = Mathf.Max(1, previewData.shuttleCapacity);
        float shuttleLoadingTime = Mathf.Max(0f, previewData.shuttleLoadingTimeSeconds);
        int activeShuttleCount = previewData.ActiveShuttleCount;
        int autoSendCount = hasMiningPlatform ? previewData.ActiveAutoSendShuttleCount : 0;
        ShuttleState[] offlineShuttles = new ShuttleState[GameData.MaxShuttles];

        for (int i = 0; i < GameData.MaxShuttles; i++)
        {
            offlineShuttles[i] = previewData.GetShuttleState(i).Clone();
        }

        int minedOre = 0;
        int warehouseOre = 0;
        long remainingSeconds = offlineSeconds;

        while (remainingSeconds > 0)
        {
            int currentOrePerSecond = GetOfflineBoostedOrePerSecond(baseOrePerSecond, offlineBoosts);
            float currentShuttleTravelTime = GetOfflineBoostedTravelTime(baseShuttleTravelTime, offlineBoosts);
            long nextBoostExpirationSeconds = GetNextOfflineBoostExpirationSeconds(offlineBoosts);

            if (hasMiningPlatform &&
                TryStartOfflineAutoActions(
                    offlineShuttles,
                    activeShuttleCount,
                    autoSendCount,
                    ref storedPlatformOre,
                    platformCapacity,
                    shuttleCapacity,
                    shuttleLoadingTime,
                    currentShuttleTravelTime,
                    ref warehouseOre))
            {
                continue;
            }

            long nextLoadingEventSeconds = GetNextOfflineLoadingEventSeconds(offlineShuttles, activeShuttleCount);

            if (nextLoadingEventSeconds > 0)
            {
                long nextTravelDuringLoadingSeconds = GetNextOfflineTravelEventSeconds(offlineShuttles, activeShuttleCount);
                long nextLoadingStepSeconds = Math.Min(remainingSeconds, nextLoadingEventSeconds);

                if (nextTravelDuringLoadingSeconds > 0)
                {
                    nextLoadingStepSeconds = Math.Min(nextLoadingStepSeconds, nextTravelDuringLoadingSeconds);
                }

                if (nextBoostExpirationSeconds > 0)
                {
                    nextLoadingStepSeconds = Math.Min(nextLoadingStepSeconds, nextBoostExpirationSeconds);
                }

                if (nextLoadingStepSeconds <= 0)
                {
                    nextLoadingStepSeconds = 1;
                }

                AdvanceOfflineLoading(
                    offlineShuttles,
                    activeShuttleCount,
                    nextLoadingStepSeconds,
                    shuttleLoadingTime,
                    shuttleCapacity,
                    currentShuttleTravelTime,
                    ref storedPlatformOre,
                    ref warehouseOre);
                AdvanceOfflineTravel(offlineShuttles, activeShuttleCount, nextLoadingStepSeconds, ref warehouseOre);
                AdvanceOfflineBoosts(offlineBoosts, nextLoadingStepSeconds);
                remainingSeconds -= nextLoadingStepSeconds;
                continue;
            }

            long nextTravelEventSeconds = GetNextOfflineTravelEventSeconds(offlineShuttles, activeShuttleCount);
            long nextAutoReadySeconds = GetNextOfflineAutoReadySeconds(
                offlineShuttles,
                activeShuttleCount,
                autoSendCount,
                storedPlatformOre,
                platformCapacity,
                shuttleCapacity,
                currentOrePerSecond);

            bool hasTimedEvent = false;
            long nextEventSeconds = remainingSeconds;

            if (nextTravelEventSeconds > 0)
            {
                nextEventSeconds = Math.Min(nextEventSeconds, nextTravelEventSeconds);
                hasTimedEvent = true;
            }

            if (nextAutoReadySeconds > 0)
            {
                nextEventSeconds = Math.Min(nextEventSeconds, nextAutoReadySeconds);
                hasTimedEvent = true;
            }

            if (nextBoostExpirationSeconds > 0)
            {
                nextEventSeconds = Math.Min(nextEventSeconds, nextBoostExpirationSeconds);
                hasTimedEvent = true;
            }

            if (!hasTimedEvent)
            {
                if (currentOrePerSecond <= 0)
                {
                    break;
                }

                int freeCapacity = Mathf.Max(0, platformCapacity - storedPlatformOre);

                if (freeCapacity <= 0)
                {
                    break;
                }

                int minedAmount = (int)Math.Min((long)freeCapacity, remainingSeconds * currentOrePerSecond);
                storedPlatformOre += minedAmount;
                minedOre += minedAmount;
                AdvanceOfflineBoosts(offlineBoosts, remainingSeconds);
                break;
            }

            if (nextEventSeconds <= 0)
            {
                nextEventSeconds = 1;
            }

            if (hasMiningPlatform && currentOrePerSecond > 0 && storedPlatformOre < platformCapacity)
            {
                int freeCapacity = Mathf.Max(0, platformCapacity - storedPlatformOre);
                int minedAmount = (int)Math.Min((long)freeCapacity, nextEventSeconds * currentOrePerSecond);
                storedPlatformOre += minedAmount;
                minedOre += minedAmount;
            }

            AdvanceOfflineTravel(offlineShuttles, activeShuttleCount, nextEventSeconds, ref warehouseOre);
            AdvanceOfflineBoosts(offlineBoosts, nextEventSeconds);
            remainingSeconds -= nextEventSeconds;
        }

        return new OfflineProgress
        {
            warehouseOre = warehouseOre,
            previewData = BuildOfflinePreviewData(
                previewData,
                storedPlatformOre,
                offlineShuttles,
                minedOre,
                warehouseOre),
            shouldShowPopup = shouldShowPopupForDuration && autoSendCount > 0 && warehouseOre > 0
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

    private void AdvanceTemporaryBoostTimersForInactiveTime()
    {
        if (upgradeManager == null)
        {
            return;
        }

        long inactiveSeconds = GetOfflineSeconds();

        if (inactiveSeconds <= 0)
        {
            return;
        }

        upgradeManager.AdvanceTime(inactiveSeconds);
    }

    private void AdvanceBoostOfferAutoCloseForInactiveTime()
    {
        if (upgradeManager == null || pendingBoostOfferStates.Count <= 0)
        {
            return;
        }

        long inactiveSeconds = GetOfflineSeconds();

        if (inactiveSeconds <= 0)
        {
            return;
        }

        for (int i = pendingBoostOfferStates.Count - 1; i >= 0; i--)
        {
            TemporaryBoostState boostState = pendingBoostOfferStates[i];

            if (boostState == null || !boostOfferAutoCloseTimers.ContainsKey(boostState))
            {
                continue;
            }

            boostOfferAutoCloseTimers[boostState] -= inactiveSeconds;

            if (boostOfferAutoCloseTimers[boostState] > 0f)
            {
                continue;
            }

            upgradeManager.TryDeclineTemporaryBoost(boostState);
            pendingBoostOfferStates.RemoveAt(i);
            boostOfferAutoCloseTimers.Remove(boostState);
        }
    }

    private void HandleApplicationSentToBackground()
    {
        if (isApplicationInBackground)
        {
            return;
        }

        isApplicationInBackground = true;
        suppressBoostOfferPopup = true;
        uiManager?.HideBoostOffer();
        SaveGame();
    }

    private void HandleApplicationReturnedToForeground()
    {
        if (!isApplicationInBackground)
        {
            return;
        }

        isApplicationInBackground = false;
        suppressBoostOfferPopup = false;

        CacheOfflineProgress(CalculateOfflineProgress());
        AdvanceTemporaryBoostTimersForInactiveTime();
        AdvanceBoostOfferAutoCloseForInactiveTime();
        SyncPendingOfflinePreviewWithCurrentDynamicStats();

        if (!ShouldShowOfflineRewardPopup())
        {
            if (HasPendingOfflineStateChanges())
            {
                ApplyPendingOfflineProgress();
            }

            SyncMissionProgress();
            SaveGame();
        }

        RefreshUI();
        ShowOfflineRewardIfNeeded();
        TryShowNextBoostOffer();
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

    private string GetConfiguredNoMissionsText()
    {
        return gameConfig != null
            ? gameConfig.NoMissionsText
            : ShuttleConfig.DefaultNoMissionsText;
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

    private void HandleShuttleSendRequested(int shuttleIndex)
    {
        OnSendShuttleButtonClicked(shuttleIndex);
    }

    private void HandleExitRequested()
    {
        OnExitGameButtonClicked();
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
        if (upgradeManager == null || pendingBoostOfferStates.Count <= 0)
        {
            return false;
        }

        bool hasChanges = false;

        for (int i = pendingBoostOfferStates.Count - 1; i >= 0; i--)
        {
            TemporaryBoostState boostState = pendingBoostOfferStates[i];

            if (boostState == null || !boostOfferAutoCloseTimers.ContainsKey(boostState))
            {
                continue;
            }

            boostOfferAutoCloseTimers[boostState] -= deltaTime;

            if (boostOfferAutoCloseTimers[boostState] > 0f)
            {
                continue;
            }

            upgradeManager.TryDeclineTemporaryBoost(boostState);
            pendingBoostOfferStates.RemoveAt(i);
            boostOfferAutoCloseTimers.Remove(boostState);
            hasChanges = true;
        }

        if (!hasChanges)
        {
            return false;
        }

        RefreshBoostOfferButtons();
        return true;
    }

    private void OnDestroy()
    {
        if (upgradeManager != null)
        {
            upgradeManager.UpgradesChanged -= HandleUpgradesChanged;
        }
    }

    private void PrepareMainPanelOpen()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideMenu();
        uiManager.HideResetConfirmation();
    }

    private bool TryAutoSendShuttle()
    {
        if (shuttleSystem == null || gameData.ActiveAutoSendShuttleCount <= 0)
        {
            return false;
        }

        bool hasChanges = false;

        for (int i = 0; i < gameData.ActiveAutoSendShuttleCount; i++)
        {
            if (!shuttleSystem.IsReadyForAutoSend(i))
            {
                continue;
            }

            hasChanges = shuttleSystem.AutoSendToWarehouse(i) > 0 || hasChanges;
        }

        return hasChanges;
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

    private void SyncPendingOfflinePreviewWithCurrentDynamicStats()
    {
        if (pendingOfflinePreviewData == null || gameData == null)
        {
            return;
        }

        pendingOfflinePreviewData.orePerClick = gameData.orePerClick;
        pendingOfflinePreviewData.orePerSecond = gameData.orePerSecond;
        pendingOfflinePreviewData.shuttleTravelTimeSeconds = gameData.shuttleTravelTimeSeconds;
    }

    private void SyncPendingBoostOffers()
    {
        if (upgradeManager == null)
        {
            pendingBoostOfferStates.Clear();
            boostOfferAutoCloseTimers.Clear();
            return;
        }

        IReadOnlyList<TemporaryBoostState> availableBoostStates = upgradeManager.GetAvailableTemporaryBoosts();

        for (int i = pendingBoostOfferStates.Count - 1; i >= 0; i--)
        {
            TemporaryBoostState boostState = pendingBoostOfferStates[i];
            bool isStillAvailable = false;

            for (int availableIndex = 0; availableIndex < availableBoostStates.Count; availableIndex++)
            {
                if (availableBoostStates[availableIndex] == boostState)
                {
                    isStillAvailable = true;
                    break;
                }
            }

            if (isStillAvailable)
            {
                continue;
            }

            pendingBoostOfferStates.RemoveAt(i);
            boostOfferAutoCloseTimers.Remove(boostState);
        }

        for (int i = 0; i < availableBoostStates.Count; i++)
        {
            TemporaryBoostState boostState = availableBoostStates[i];

            if (pendingBoostOfferStates.Contains(boostState))
            {
                continue;
            }

            pendingBoostOfferStates.Add(boostState);
            boostOfferAutoCloseTimers[boostState] = GetConfiguredBoostOfferAutoCloseSeconds();
        }
    }

    private void RefreshBoostOfferButtons()
    {
        if (uiManager == null)
        {
            return;
        }

        if (pendingBoostOfferStates.Count <= 0)
        {
            uiManager.HideBoostOffer();
            return;
        }

        uiManager.ShowBoostOffers(pendingBoostOfferStates);
    }

    private void RemovePendingBoostOffer(TemporaryBoostState boostState)
    {
        if (boostState == null)
        {
            return;
        }

        pendingBoostOfferStates.Remove(boostState);
        boostOfferAutoCloseTimers.Remove(boostState);

        if (pendingBoostOfferStates.Count <= 0)
        {
            uiManager?.HideBoostOffer();
        }
    }

    private void TryShowNextBoostOffer()
    {
        if (uiManager == null ||
            upgradeManager == null ||
            suppressBoostOfferPopup ||
            uiManager.IsBusyWithOtherWindow)
        {
            return;
        }

        SyncPendingBoostOffers();
        RefreshBoostOfferButtons();
    }

    private void DismissPendingBoostOffer()
    {
        if (uiManager != null)
        {
            uiManager.HideBoostOffer();
        }

        if (upgradeManager == null || pendingBoostOfferStates.Count <= 0)
        {
            pendingBoostOfferStates.Clear();
            boostOfferAutoCloseTimers.Clear();
            return;
        }

        for (int i = 0; i < pendingBoostOfferStates.Count; i++)
        {
            upgradeManager.TryDeclineTemporaryBoost(pendingBoostOfferStates[i]);
        }

        pendingBoostOfferStates.Clear();
        boostOfferAutoCloseTimers.Clear();
    }

    private void DismissBoostOfferAndRefresh()
    {
        if (pendingBoostOfferStates.Count <= 0 && (uiManager == null || !uiManager.IsBoostOfferVisible))
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
               pendingOfflinePreviewData.totalOreEarned != gameData.totalOreEarned ||
               HasPendingShuttleStateChanges();
    }

    private GameData BuildOfflinePreviewData(
        GameData previewData,
        int shuttleOre,
        ShuttleState[] shuttleStates,
        int minedOre,
        int warehouseOre)
    {
        previewData.shuttleOre = Mathf.Max(0, shuttleOre);

        for (int i = 0; i < GameData.MaxShuttles; i++)
        {
            previewData.GetShuttleState(i).CopyFrom(shuttleStates[i]);
        }

        previewData.ore += warehouseOre;
        previewData.totalOreEarned += minedOre;
        return previewData;
    }

    private bool IsOfflineShuttleBusy(ShuttleState shuttleState)
    {
        return shuttleState.loadingCooldownRemaining > 0f ||
               shuttleState.loadingTargetOre > 0 ||
               shuttleState.loadingOre > 0 ||
               shuttleState.sendCooldownRemaining > 0f ||
               shuttleState.deliveringOre > 0;
    }

    private long GetNextOfflineLoadingEventSeconds(ShuttleState[] shuttleStates, int activeShuttleCount)
    {
        long nextEventSeconds = 0;

        for (int i = 0; i < activeShuttleCount; i++)
        {
            float loadingCooldown = shuttleStates[i].loadingCooldownRemaining;

            if (loadingCooldown <= 0f)
            {
                continue;
            }

            long eventSeconds = (long)Math.Ceiling(loadingCooldown);

            if (nextEventSeconds == 0 || eventSeconds < nextEventSeconds)
            {
                nextEventSeconds = eventSeconds;
            }
        }

        return nextEventSeconds;
    }

    private void AdvanceOfflineLoading(
        ShuttleState[] shuttleStates,
        int activeShuttleCount,
        long deltaSeconds,
        float shuttleLoadingTime,
        int shuttleCapacity,
        float shuttleTravelTime,
        ref int storedPlatformOre,
        ref int warehouseOre)
    {
        if (deltaSeconds <= 0)
        {
            return;
        }

        for (int i = 0; i < activeShuttleCount; i++)
        {
            ShuttleState shuttleState = shuttleStates[i];

            if (shuttleState.loadingCooldownRemaining <= 0f || shuttleState.loadingTargetOre <= 0)
            {
                continue;
            }

            shuttleState.loadingCooldownRemaining = Mathf.Max(0f, shuttleState.loadingCooldownRemaining - deltaSeconds);

            int desiredLoadedOre = GetOfflineLoadedOre(
                shuttleState.loadingTargetOre,
                shuttleLoadingTime,
                shuttleState.loadingCooldownRemaining);
            int deltaLoadedOre = Math.Max(0, desiredLoadedOre - shuttleState.loadingOre);

            if (deltaLoadedOre <= 0)
            {
                continue;
            }

            int transferredOre = Math.Min(deltaLoadedOre, storedPlatformOre);
            storedPlatformOre -= transferredOre;
            shuttleState.loadingOre += transferredOre;
        }

        for (int i = 0; i < activeShuttleCount; i++)
        {
            ShuttleState shuttleState = shuttleStates[i];

            if (shuttleState.loadingCooldownRemaining > 0f || shuttleState.loadingTargetOre <= 0)
            {
                continue;
            }

            int remainingLoadOre = Math.Min(
                Math.Max(0, shuttleState.loadingTargetOre - shuttleState.loadingOre),
                storedPlatformOre);
            storedPlatformOre -= remainingLoadOre;
            shuttleState.loadingOre += remainingLoadOre;
            shuttleState.dockedOre = Mathf.Min(shuttleCapacity, shuttleState.dockedOre + shuttleState.loadingOre);

            shuttleState.loadingOre = 0;
            shuttleState.loadingTargetOre = 0;
            shuttleState.loadingCooldownRemaining = 0f;

            if (!shuttleState.sendAfterLoading)
            {
                continue;
            }

            int cargoToSend = shuttleState.dockedOre;
            shuttleState.dockedOre = 0;
            shuttleState.sendAfterLoading = false;
            StartOfflineTravel(shuttleState, cargoToSend, shuttleTravelTime, ref warehouseOre);
        }
    }

    private bool TryStartOfflineAutoActions(
        ShuttleState[] shuttleStates,
        int activeShuttleCount,
        int autoSendCount,
        ref int storedPlatformOre,
        int platformCapacity,
        int shuttleCapacity,
        float shuttleLoadingTime,
        float shuttleTravelTime,
        ref int warehouseOre)
    {
        bool startedAnyAction = false;
        bool startedActionThisPass;

        do
        {
            startedActionThisPass = false;

            for (int i = 0; i < Mathf.Min(activeShuttleCount, autoSendCount); i++)
            {
                ShuttleState shuttleState = shuttleStates[i];

                if (IsOfflineShuttleBusy(shuttleState))
                {
                    continue;
                }

                if (shuttleState.dockedOre >= shuttleCapacity)
                {
                    int cargoToSend = shuttleState.dockedOre;
                    shuttleState.dockedOre = 0;
                    StartOfflineTravel(shuttleState, cargoToSend, shuttleTravelTime, ref warehouseOre);
                    startedAnyAction = true;
                    startedActionThisPass = true;
                    continue;
                }

                int autoSendThreshold = GetOfflineAutoSendThreshold(platformCapacity, shuttleCapacity, shuttleState.dockedOre);
                int availablePlatformOre = GetOfflineAvailablePlatformOreForLoading(
                    shuttleStates,
                    activeShuttleCount,
                    storedPlatformOre,
                    i);

                if (autoSendThreshold <= 0 || availablePlatformOre < autoSendThreshold)
                {
                    continue;
                }

                int transferAmount = Mathf.Min(
                    availablePlatformOre,
                    Mathf.Max(0, shuttleCapacity - shuttleState.dockedOre));

                if (transferAmount <= 0)
                {
                    continue;
                }

                if (shuttleLoadingTime > 0f)
                {
                    shuttleState.loadingTargetOre = transferAmount;
                    shuttleState.loadingOre = 0;
                    shuttleState.loadingCooldownRemaining = shuttleLoadingTime;
                    shuttleState.sendAfterLoading = shuttleState.dockedOre + transferAmount >= shuttleCapacity;
                    startedAnyAction = true;
                    startedActionThisPass = true;
                    continue;
                }

                storedPlatformOre -= transferAmount;
                shuttleState.dockedOre = Mathf.Min(shuttleCapacity, shuttleState.dockedOre + transferAmount);

                if (shuttleState.dockedOre >= shuttleCapacity)
                {
                    int cargoToSend = shuttleState.dockedOre;
                    shuttleState.dockedOre = 0;
                    StartOfflineTravel(shuttleState, cargoToSend, shuttleTravelTime, ref warehouseOre);
                }

                startedAnyAction = true;
                startedActionThisPass = true;
            }
        }
        while (startedActionThisPass);

        return startedAnyAction;
    }

    private int GetOfflineAvailablePlatformOreForLoading(
        ShuttleState[] shuttleStates,
        int activeShuttleCount,
        int storedPlatformOre,
        int shuttleIndex)
    {
        int reservedOre = 0;

        for (int i = 0; i < activeShuttleCount; i++)
        {
            if (i == shuttleIndex)
            {
                continue;
            }

            reservedOre += Math.Max(0, shuttleStates[i].loadingTargetOre - shuttleStates[i].loadingOre);
        }

        return Math.Max(0, storedPlatformOre - reservedOre);
    }

    private int GetOfflineAutoSendThreshold(int platformCapacity, int shuttleCapacity, int currentDockedOre)
    {
        int remainingShuttleCapacity = Mathf.Max(0, shuttleCapacity - currentDockedOre);

        if (remainingShuttleCapacity <= 0)
        {
            return 0;
        }

        return Mathf.Max(1, Mathf.Min(platformCapacity, remainingShuttleCapacity));
    }

    private void StartOfflineTravel(ShuttleState shuttleState, int cargoAmount, float shuttleTravelTime, ref int warehouseOre)
    {
        if (cargoAmount <= 0)
        {
            shuttleState.deliveringOre = 0;
            shuttleState.sendCooldownRemaining = 0f;
            return;
        }

        if (shuttleTravelTime <= 0f)
        {
            warehouseOre += cargoAmount;
            shuttleState.deliveringOre = 0;
            shuttleState.sendCooldownRemaining = 0f;
            return;
        }

        shuttleState.deliveringOre = cargoAmount;
        shuttleState.sendCooldownRemaining = shuttleTravelTime;
    }

    private long GetNextOfflineTravelEventSeconds(ShuttleState[] shuttleStates, int activeShuttleCount)
    {
        long nextEventSeconds = 0;

        for (int i = 0; i < activeShuttleCount; i++)
        {
            float travelCooldown = shuttleStates[i].sendCooldownRemaining;

            if (travelCooldown <= 0f)
            {
                continue;
            }

            long eventSeconds = (long)Math.Ceiling(travelCooldown);

            if (nextEventSeconds == 0 || eventSeconds < nextEventSeconds)
            {
                nextEventSeconds = eventSeconds;
            }
        }

        return nextEventSeconds;
    }

    private long GetNextOfflineAutoReadySeconds(
        ShuttleState[] shuttleStates,
        int activeShuttleCount,
        int autoSendCount,
        int storedPlatformOre,
        int platformCapacity,
        int shuttleCapacity,
        int orePerSecond)
    {
        if (orePerSecond <= 0)
        {
            return 0;
        }

        long nextReadySeconds = 0;

        for (int i = 0; i < Mathf.Min(activeShuttleCount, autoSendCount); i++)
        {
            ShuttleState shuttleState = shuttleStates[i];

            if (IsOfflineShuttleBusy(shuttleState))
            {
                continue;
            }

            int autoSendThreshold = GetOfflineAutoSendThreshold(platformCapacity, shuttleCapacity, shuttleState.dockedOre);

            if (autoSendThreshold <= 0 || storedPlatformOre >= autoSendThreshold)
            {
                continue;
            }

            int requiredOre = autoSendThreshold - storedPlatformOre;
            long readySeconds = Math.Max(1L, Mathf.CeilToInt(requiredOre / (float)orePerSecond));

            if (nextReadySeconds == 0 || readySeconds < nextReadySeconds)
            {
                nextReadySeconds = readySeconds;
            }
        }

        return nextReadySeconds;
    }

    private void AdvanceOfflineTravel(ShuttleState[] shuttleStates, int activeShuttleCount, long deltaSeconds, ref int warehouseOre)
    {
        for (int i = 0; i < activeShuttleCount; i++)
        {
            ShuttleState shuttleState = shuttleStates[i];

            if (shuttleState.sendCooldownRemaining <= 0f)
            {
                continue;
            }

            shuttleState.sendCooldownRemaining = Mathf.Max(0f, shuttleState.sendCooldownRemaining - deltaSeconds);

            if (shuttleState.sendCooldownRemaining <= 0f && shuttleState.deliveringOre > 0)
            {
                warehouseOre += shuttleState.deliveringOre;
                shuttleState.deliveringOre = 0;
            }
        }
    }

    private bool HasPendingShuttleStateChanges()
    {
        for (int i = 0; i < GameData.MaxShuttles; i++)
        {
            ShuttleState pendingState = pendingOfflinePreviewData.GetShuttleState(i);
            ShuttleState currentState = gameData.GetShuttleState(i);

            if (pendingState.dockedOre != currentState.dockedOre ||
                pendingState.loadingOre != currentState.loadingOre ||
                pendingState.loadingTargetOre != currentState.loadingTargetOre ||
                pendingState.sendAfterLoading != currentState.sendAfterLoading ||
                pendingState.deliveringOre != currentState.deliveringOre ||
                !Mathf.Approximately(pendingState.loadingCooldownRemaining, currentState.loadingCooldownRemaining) ||
                !Mathf.Approximately(pendingState.sendCooldownRemaining, currentState.sendCooldownRemaining))
            {
                return true;
            }
        }

        return pendingOfflinePreviewData.shuttleCount != gameData.shuttleCount ||
               pendingOfflinePreviewData.shuttleAutoSendCount != gameData.shuttleAutoSendCount;
    }

    private List<OfflineBoostSnapshot> CreateOfflineBoostSnapshots()
    {
        List<OfflineBoostSnapshot> snapshots = new List<OfflineBoostSnapshot>();

        if (upgradeManager == null)
        {
            return snapshots;
        }

        IReadOnlyList<TemporaryBoostState> activeBoostStates = upgradeManager.ActiveTemporaryBoostStates;

        for (int i = 0; i < activeBoostStates.Count; i++)
        {
            TemporaryBoostState boostState = activeBoostStates[i];

            if (!boostState.IsActive || boostState.ActiveRemainingTime <= 0f)
            {
                continue;
            }

            snapshots.Add(new OfflineBoostSnapshot
            {
                targetType = boostState.Definition.targetType,
                multiplier = boostState.GetMultiplier(),
                remainingTime = boostState.ActiveRemainingTime
            });
        }

        return snapshots;
    }

    private float GetOfflineBoostMultiplier(
        List<OfflineBoostSnapshot> boostSnapshots,
        TemporaryBoostTargetType targetType)
    {
        float multiplier = 1f;

        if (boostSnapshots == null)
        {
            return multiplier;
        }

        for (int i = 0; i < boostSnapshots.Count; i++)
        {
            OfflineBoostSnapshot boostSnapshot = boostSnapshots[i];

            if (boostSnapshot.targetType != targetType || boostSnapshot.remainingTime <= 0f)
            {
                continue;
            }

            multiplier *= Mathf.Max(1f, boostSnapshot.multiplier);
        }

        return multiplier;
    }

    private long GetNextOfflineBoostExpirationSeconds(List<OfflineBoostSnapshot> boostSnapshots)
    {
        if (boostSnapshots == null || boostSnapshots.Count <= 0)
        {
            return 0;
        }

        long nextExpirationSeconds = 0;

        for (int i = 0; i < boostSnapshots.Count; i++)
        {
            float remainingTime = boostSnapshots[i].remainingTime;

            if (remainingTime <= 0f)
            {
                continue;
            }

            long expirationSeconds = Math.Max(1L, (long)Math.Ceiling(remainingTime));

            if (nextExpirationSeconds == 0 || expirationSeconds < nextExpirationSeconds)
            {
                nextExpirationSeconds = expirationSeconds;
            }
        }

        return nextExpirationSeconds;
    }

    private void AdvanceOfflineBoosts(List<OfflineBoostSnapshot> boostSnapshots, long deltaSeconds)
    {
        if (boostSnapshots == null || deltaSeconds <= 0)
        {
            return;
        }

        for (int i = boostSnapshots.Count - 1; i >= 0; i--)
        {
            OfflineBoostSnapshot boostSnapshot = boostSnapshots[i];
            boostSnapshot.remainingTime = Mathf.Max(0f, boostSnapshot.remainingTime - deltaSeconds);

            if (boostSnapshot.remainingTime <= 0f)
            {
                boostSnapshots.RemoveAt(i);
                continue;
            }

            boostSnapshots[i] = boostSnapshot;
        }
    }

    private int GetOfflineBoostedOrePerSecond(int baseOrePerSecond, List<OfflineBoostSnapshot> boostSnapshots)
    {
        float multiplier = GetOfflineBoostMultiplier(boostSnapshots, TemporaryBoostTargetType.OrePerSecond);
        return Mathf.Max(0, Mathf.RoundToInt(baseOrePerSecond * multiplier));
    }

    private float GetOfflineBoostedTravelTime(float baseTravelTime, List<OfflineBoostSnapshot> boostSnapshots)
    {
        float speedMultiplier = GetOfflineBoostMultiplier(boostSnapshots, TemporaryBoostTargetType.ShuttleTravelSpeed);
        return Mathf.Max(0f, baseTravelTime / Mathf.Max(1f, speedMultiplier));
    }

    private int RemoveTemporaryBoostMultiplier(int boostedValue, float multiplier)
    {
        if (multiplier <= 1f)
        {
            return Mathf.Max(0, boostedValue);
        }

        return Mathf.Max(0, Mathf.RoundToInt(boostedValue / multiplier));
    }

    private float RemoveTemporaryTravelSpeedMultiplier(float boostedTravelTime, float speedMultiplier)
    {
        if (speedMultiplier <= 1f)
        {
            return Mathf.Max(0f, boostedTravelTime);
        }

        return Mathf.Max(0f, boostedTravelTime * speedMultiplier);
    }

    private void SaveShuttleState(int shuttleIndex, ShuttleState shuttleState)
    {
        if (shuttleState == null)
        {
            return;
        }

        PlayerPrefs.SetInt(GetShuttleStateKey(shuttleIndex, "dockedOre"), Mathf.Max(0, shuttleState.dockedOre));
        PlayerPrefs.SetInt(GetShuttleStateKey(shuttleIndex, "loadingOre"), Mathf.Max(0, shuttleState.loadingOre));
        PlayerPrefs.SetInt(GetShuttleStateKey(shuttleIndex, "loadingTargetOre"), Mathf.Max(shuttleState.loadingOre, shuttleState.loadingTargetOre));
        PlayerPrefs.SetInt(GetShuttleStateKey(shuttleIndex, "sendAfterLoading"), shuttleState.sendAfterLoading ? 1 : 0);
        PlayerPrefs.SetInt(GetShuttleStateKey(shuttleIndex, "deliveringOre"), Mathf.Max(0, shuttleState.deliveringOre));
        PlayerPrefs.SetFloat(GetShuttleStateKey(shuttleIndex, "loadingCooldown"), Mathf.Max(0f, shuttleState.loadingCooldownRemaining));
        PlayerPrefs.SetFloat(GetShuttleStateKey(shuttleIndex, "travelCooldown"), Mathf.Max(0f, shuttleState.sendCooldownRemaining));
    }

    private void LoadShuttleState(int shuttleIndex, ShuttleState shuttleState)
    {
        if (shuttleState == null)
        {
            return;
        }

        if (shuttleIndex == 0)
        {
            shuttleState.dockedOre = Mathf.Clamp(
                PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "dockedOre"), gameData.shuttleDockedOre),
                0,
                gameData.shuttleCapacity);
            shuttleState.loadingOre = Mathf.Max(
                0,
                PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "loadingOre"), gameData.shuttleLoadingOre));
            shuttleState.loadingTargetOre = Mathf.Max(
                shuttleState.loadingOre,
                PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "loadingTargetOre"), gameData.shuttleLoadingTargetOre));
            shuttleState.sendAfterLoading = PlayerPrefs.GetInt(
                GetShuttleStateKey(shuttleIndex, "sendAfterLoading"),
                gameData.shuttleSendAfterLoading ? 1 : 0) == 1;
            shuttleState.deliveringOre = Mathf.Max(
                0,
                PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "deliveringOre"), gameData.shuttleDeliveringOre));
            shuttleState.loadingCooldownRemaining = Mathf.Max(
                0f,
                PlayerPrefs.GetFloat(GetShuttleStateKey(shuttleIndex, "loadingCooldown"), gameData.shuttleLoadingCooldownRemaining));
            shuttleState.sendCooldownRemaining = Mathf.Max(
                0f,
                PlayerPrefs.GetFloat(GetShuttleStateKey(shuttleIndex, "travelCooldown"), gameData.shuttleSendCooldownRemaining));
            return;
        }

        shuttleState.dockedOre = Mathf.Clamp(
            PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "dockedOre"), 0),
            0,
            gameData.shuttleCapacity);
        shuttleState.loadingOre = Mathf.Max(0, PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "loadingOre"), 0));
        shuttleState.loadingTargetOre = Mathf.Max(
            shuttleState.loadingOre,
            PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "loadingTargetOre"), 0));
        shuttleState.sendAfterLoading = PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "sendAfterLoading"), 0) == 1;
        shuttleState.deliveringOre = Mathf.Max(0, PlayerPrefs.GetInt(GetShuttleStateKey(shuttleIndex, "deliveringOre"), 0));
        shuttleState.loadingCooldownRemaining = Mathf.Max(0f, PlayerPrefs.GetFloat(GetShuttleStateKey(shuttleIndex, "loadingCooldown"), 0f));
        shuttleState.sendCooldownRemaining = Mathf.Max(0f, PlayerPrefs.GetFloat(GetShuttleStateKey(shuttleIndex, "travelCooldown"), 0f));
    }

    private void DeleteShuttleState(int shuttleIndex)
    {
        PlayerPrefs.DeleteKey(GetShuttleStateKey(shuttleIndex, "dockedOre"));
        PlayerPrefs.DeleteKey(GetShuttleStateKey(shuttleIndex, "loadingOre"));
        PlayerPrefs.DeleteKey(GetShuttleStateKey(shuttleIndex, "loadingTargetOre"));
        PlayerPrefs.DeleteKey(GetShuttleStateKey(shuttleIndex, "sendAfterLoading"));
        PlayerPrefs.DeleteKey(GetShuttleStateKey(shuttleIndex, "deliveringOre"));
        PlayerPrefs.DeleteKey(GetShuttleStateKey(shuttleIndex, "loadingCooldown"));
        PlayerPrefs.DeleteKey(GetShuttleStateKey(shuttleIndex, "travelCooldown"));
    }

    private string GetShuttleStateKey(int shuttleIndex, string suffix)
    {
        return ShuttleStateKeyPrefix + shuttleIndex + "_" + suffix;
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

    private struct OfflineBoostSnapshot
    {
        public TemporaryBoostTargetType targetType;
        public float multiplier;
        public float remainingTime;
    }
}
