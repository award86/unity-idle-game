using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private const string OreKey = "ore";
    private const string ResourceKeyPrefix = "resource_";
    private const string ShuttleOreKey = "shuttleOre";
    private const string ShuttleDeliveringOreKey = "shuttleDeliveringOre";
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
    [SerializeField] private TemporaryBoostConfig temporaryBoostConfig;

    private GameData gameData;
    private ShuttleSystem shuttleSystem;
    private ResourceSystem resourceSystem;
    private UpgradeManager upgradeManager;
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

        shuttleSystem = new ShuttleSystem(gameData);
        resourceSystem = new ResourceSystem(gameData, shuttleSystem);
        upgradeManager = new UpgradeManager(gameData, resourceSystem, gameConfig, upgradeConfig, temporaryBoostConfig);
        upgradeManager.LoadUpgradeLevels();
        upgradeManager.UpgradesChanged += HandleUpgradesChanged;
        timeSystem = new TimeSystem(resourceSystem);
        energySystem = new EnergySystem(gameData, resourceSystem);
        CacheOfflineProgress(CalculateOfflineProgress());
    }

    private void Start()
    {
        if (uiManager != null)
        {
            uiManager.InitializeUpgradeList(
                upgradeManager.UpgradeStates,
                HandleUpgradeBuyRequested);
        }

        if (!ShouldShowOfflineRewardPopup() && HasPendingOfflineStateChanges())
        {
            ApplyPendingOfflineProgress();
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

        if (shuttleSystem != null && shuttleSystem.UpdateCooldown(Time.deltaTime))
        {
            shouldRefreshUi = true;
        }

        if (TryAutoSendShuttle())
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
        RefreshUI();
    }

    public void OnProduceMetalButtonClicked()
    {
        if (resourceSystem == null || !resourceSystem.TryProduceMetal())
        {
            return;
        }

        RefreshUI();
        SaveGame();
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
        TryShowNextBoostOffer();
    }

    public void OnUpgradeCategoryTabClicked(int categoryIndex)
    {
        if (uiManager == null || !Enum.IsDefined(typeof(UpgradeCategory), categoryIndex))
        {
            return;
        }

        uiManager.SetUpgradeCategory((UpgradeCategory)categoryIndex);
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

    public void OnDeclineBoostButtonClicked()
    {
        DismissBoostOfferAndRefresh();
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
        PlayerPrefs.SetInt(ShuttleDeliveringOreKey, gameData.shuttleDeliveringOre);
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

        PlayerPrefs.SetString(LastSaveTimeKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        gameData = new GameData();

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

        gameData.ore = GetSavedResourceAmount(ResourceType.Ore, GetConfiguredStartOre(), OreKey);
        gameData.energy = GetSavedResourceAmount(ResourceType.Energy, GetConfiguredStartEnergy());
        gameData.metal = GetSavedResourceAmount(ResourceType.Metal, GetConfiguredStartMetal());
        gameData.crystal = GetSavedResourceAmount(ResourceType.Crystal, GetConfiguredStartCrystal());
        gameData.shuttleOre = loadedShuttleOre;
        gameData.shuttleDeliveringOre = loadedShuttleDeliveringOre;
        gameData.shuttleCapacity = loadedShuttleCapacity;
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
        gameData.shuttleDeliveringOre = 0;
        gameData.shuttleCapacity = GetConfiguredShuttleCapacity();
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
        PlayerPrefs.DeleteKey(ShuttleDeliveringOreKey);
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

        if (uiManager != null)
        {
            uiManager.HideOfflineReward();
            uiManager.HideUpgradePanel();
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
        uiManager.RefreshUpgradeList(displayData);
        uiManager.SetMainScreenUpgradeButtonVisible(upgradeManager.HasAffordableUpgrade());
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
        int shuttleCapacity = Mathf.Max(1, previewData.shuttleCapacity);
        int shuttleOre = Mathf.Max(0, previewData.shuttleOre);
        int shuttleDeliveringOre = Mathf.Max(0, previewData.shuttleDeliveringOre);
        float shuttleTravelTime = Mathf.Max(0f, previewData.shuttleTravelTimeSeconds);
        float shuttleCooldown = Mathf.Max(0f, previewData.shuttleSendCooldownRemaining);
        bool autoSendEnabled = previewData.shuttleAutoSendEnabled;
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
            warehouseOre = warehouseOre,
            previewData = BuildOfflinePreviewData(previewData, shuttleOre, shuttleDeliveringOre, shuttleCooldown, minedOre, warehouseOre),
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

    private int GetConfiguredShuttleCapacity()
    {
        return gameConfig != null
            ? gameConfig.Capacity
            : ShuttleConfig.DefaultCapacity;
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
        RefreshUI();
        SaveGame();
    }

    private void HandleUpgradesChanged()
    {
        TryAutoSendShuttle();
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

        if (!shuttleSystem.IsFull() || !shuttleSystem.CanSend())
        {
            return false;
        }

        return shuttleSystem.SendToWarehouse() > 0;
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
               pendingOfflinePreviewData.shuttleDeliveringOre != gameData.shuttleDeliveringOre ||
               !Mathf.Approximately(pendingOfflinePreviewData.shuttleSendCooldownRemaining, gameData.shuttleSendCooldownRemaining) ||
               !Mathf.Approximately(pendingOfflinePreviewData.energyRegenTimer, gameData.energyRegenTimer) ||
               pendingOfflinePreviewData.totalOreEarned != gameData.totalOreEarned;
    }

    private GameData BuildOfflinePreviewData(
        GameData previewData,
        int shuttleOre,
        int shuttleDeliveringOre,
        float shuttleCooldown,
        int minedOre,
        int warehouseOre)
    {
        previewData.shuttleOre = Mathf.Max(0, shuttleOre);
        previewData.shuttleDeliveringOre = Mathf.Max(0, shuttleDeliveringOre);
        previewData.shuttleSendCooldownRemaining = Mathf.Max(0f, shuttleCooldown);
        previewData.ore += warehouseOre;
        previewData.totalOreEarned += minedOre;
        return previewData;
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
