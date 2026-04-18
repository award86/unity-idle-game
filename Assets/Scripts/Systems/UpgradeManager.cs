using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager
{
    private const string UpgradeLevelKeyPrefix = "upgrade_level_";
    private const string BuildingLevelKeyPrefix = "building_level_";
    private const string TemporaryBoostAvailableKeyPrefix = "temporary_boost_available_";
    private const string TemporaryBoostTimeKeyPrefix = "temporary_boost_time_";
    private const string TemporaryBoostOreThresholdKeyPrefix = "temporary_boost_ore_threshold_";
    private const string TemporaryBoostActiveTimeKeyPrefix = "temporary_boost_active_time_";

    private readonly GameData gameData;
    private readonly ResourceSystem resourceSystem;
    private readonly ShuttleConfig gameConfig;
    private readonly MissionManager missionManager;
    private readonly List<UpgradeState> upgradeStates = new List<UpgradeState>();
    private readonly List<BuildingState> buildingStates = new List<BuildingState>();
    private readonly List<TemporaryBoostState> temporaryBoostStates = new List<TemporaryBoostState>();
    private readonly List<TemporaryBoostState> activeTemporaryBoostStates = new List<TemporaryBoostState>();

    public event Action UpgradesChanged;

    public UpgradeManager(
        GameData gameData,
        ResourceSystem resourceSystem,
        ShuttleConfig gameConfig,
        UpgradeConfig upgradeConfig,
        BuildingConfig buildingConfig,
        TemporaryBoostConfig temporaryBoostConfig,
        MissionManager missionManager = null)
    {
        this.gameData = gameData;
        this.resourceSystem = resourceSystem;
        this.gameConfig = gameConfig;
        this.missionManager = missionManager;
        BuildUpgradeStates(upgradeConfig);
        BuildBuildingStates(buildingConfig);
        BuildTemporaryBoostStates(temporaryBoostConfig);
        // Persistent resource values are loaded before upgrade/building levels.
        // Recalculating here would temporarily treat every unlock as level 0 and
        // clamp saved energy/platform ore down to the "no buildings" state.
        // The first real recalculation should happen after LoadUpgradeLevels().
    }

    public IReadOnlyList<UpgradeState> UpgradeStates => upgradeStates;
    public IReadOnlyList<BuildingState> BuildingStates => buildingStates;
    public IReadOnlyList<TemporaryBoostState> ActiveTemporaryBoostStates => activeTemporaryBoostStates;

    public void LoadUpgradeLevels()
    {
        activeTemporaryBoostStates.Clear();

        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];
            int savedLevel = Mathf.Max(0, PlayerPrefs.GetInt(GetUpgradeLevelKey(state.Definition.id), 0));
            state.SetLevel(savedLevel);
        }

        for (int i = 0; i < buildingStates.Count; i++)
        {
            BuildingState state = buildingStates[i];
            int savedLevel = Mathf.Max(0, PlayerPrefs.GetInt(GetBuildingLevelKey(state.Definition.id), 0));
            state.SetLevel(savedLevel);
        }

        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            TemporaryBoostState state = temporaryBoostStates[i];
            LoadTemporaryBoostState(state);

            if (state.IsActive)
            {
                activeTemporaryBoostStates.Add(state);
            }
        }

        RecalculateIncome();
        UpgradesChanged?.Invoke();
    }

    public void SaveUpgradeLevels()
    {
        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];
            PlayerPrefs.SetInt(GetUpgradeLevelKey(state.Definition.id), state.Level);
        }

        for (int i = 0; i < buildingStates.Count; i++)
        {
            BuildingState state = buildingStates[i];
            PlayerPrefs.SetInt(GetBuildingLevelKey(state.Definition.id), state.Level);
        }

        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            SaveTemporaryBoostState(temporaryBoostStates[i]);
        }
    }

    public void ResetUpgrades()
    {
        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];
            state.SetLevel(0);
            PlayerPrefs.DeleteKey(GetUpgradeLevelKey(state.Definition.id));
        }

        for (int i = 0; i < buildingStates.Count; i++)
        {
            BuildingState state = buildingStates[i];
            state.SetLevel(0);
            PlayerPrefs.DeleteKey(GetBuildingLevelKey(state.Definition.id));
        }

        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            TemporaryBoostState state = temporaryBoostStates[i];
            DeleteTemporaryBoostState(state);
            state.Deactivate();
            InitializeTemporaryBoostState(state);
        }

        activeTemporaryBoostStates.Clear();
        RecalculateIncome();
        UpgradesChanged?.Invoke();
    }

    public bool TryBuyBuilding(BuildingState state)
    {
        if (state == null || state.IsMaxLevel)
        {
            return false;
        }

        if (!state.CanAfford(gameData))
        {
            return false;
        }

        if (!resourceSystem.TrySpend(state.GetCurrentCosts()))
        {
            return false;
        }

        state.IncreaseLevel();
        RecalculateIncome();
        UpgradesChanged?.Invoke();
        return true;
    }

    public bool TryBuyUpgrade(UpgradeState state)
    {
        if (state == null || state.IsMaxLevel)
        {
            return false;
        }

        if (!state.CanAfford(gameData))
        {
            return false;
        }

        if (!resourceSystem.TrySpend(state.GetCurrentCosts()))
        {
            return false;
        }

        state.IncreaseLevel();
        RecalculateIncome();
        UpgradesChanged?.Invoke();
        return true;
    }

    public bool TryActivateTemporaryBoost(TemporaryBoostState state)
    {
        if (state == null ||
            !state.IsAvailable ||
            state.IsActive ||
            activeTemporaryBoostStates.Count >= GameSettings.MaxActiveTemporaryBoosts)
        {
            return false;
        }

        ActivateBoost(state);
        ResetTemporaryBoostAvailability(state);
        RecalculateIncome();
        UpgradesChanged?.Invoke();
        return true;
    }

    public bool TryDeclineTemporaryBoost(TemporaryBoostState state)
    {
        if (state == null || !state.IsAvailable || state.IsActive)
        {
            return false;
        }

        ResetTemporaryBoostAvailability(state);
        UpgradesChanged?.Invoke();
        return true;
    }

    public bool HasAffordableUpgrade()
    {
        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];

            if (state.IsMaxLevel)
            {
                continue;
            }

            if (!IsUpgradeCategoryUnlocked(state.Definition.ResolvedCategory))
            {
                continue;
            }

            if (state.CanAfford(gameData))
            {
                return true;
            }
        }
        return false;
    }

    public bool HasAffordableBuilding()
    {
        for (int i = 0; i < buildingStates.Count; i++)
        {
            BuildingState state = buildingStates[i];

            if (state.IsMaxLevel)
            {
                continue;
            }

            if (state.CanAfford(gameData))
            {
                return true;
            }
        }

        return false;
    }

    public UpgradeCategory GetPreferredAffordableUpgradeCategory()
    {
        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];
            UpgradeCategory category = state.Definition.ResolvedCategory;

            if (state.IsMaxLevel || !IsUpgradeCategoryUnlocked(category))
            {
                continue;
            }

            if (state.CanAfford(gameData))
            {
                return category;
            }
        }

        if (IsUpgradeCategoryUnlocked(UpgradeCategory.Miner))
        {
            return UpgradeCategory.Miner;
        }

        if (IsUpgradeCategoryUnlocked(UpgradeCategory.Platform))
        {
            return UpgradeCategory.Platform;
        }

        if (IsUpgradeCategoryUnlocked(UpgradeCategory.PowerStation))
        {
            return UpgradeCategory.PowerStation;
        }

        if (IsUpgradeCategoryUnlocked(UpgradeCategory.Factory))
        {
            return UpgradeCategory.Factory;
        }

        if (IsUpgradeCategoryUnlocked(UpgradeCategory.Shuttle))
        {
            return UpgradeCategory.Shuttle;
        }

        return UpgradeCategory.Miner;
    }

    public bool HasAnyUnlockedUpgradeCategory()
    {
        return IsUpgradeCategoryUnlocked(UpgradeCategory.Miner) ||
               IsUpgradeCategoryUnlocked(UpgradeCategory.Platform) ||
               IsUpgradeCategoryUnlocked(UpgradeCategory.PowerStation) ||
               IsUpgradeCategoryUnlocked(UpgradeCategory.Factory) ||
               IsUpgradeCategoryUnlocked(UpgradeCategory.Shuttle);
    }

    public bool IsUpgradeCategoryUnlocked(UpgradeCategory category)
    {
        switch (category)
        {
            case UpgradeCategory.Miner:
                return true;

            case UpgradeCategory.Platform:
                return HasUnlockedBuildingEffect(
                    UpgradeEffectType.PlatformCapacity,
                    UpgradeEffectType.OrePerSecond);

            case UpgradeCategory.PowerStation:
                return HasUnlockedBuildingEffect(
                    UpgradeEffectType.EnergyCapacity,
                    UpgradeEffectType.EnergyRegenAmount,
                    UpgradeEffectType.EnergyRegenIntervalReduction);

            case UpgradeCategory.Factory:
                return HasUnlockedBuildingEffect(
                    UpgradeEffectType.MetalProductionAmount,
                    UpgradeEffectType.MetalOreCostReduction,
                    UpgradeEffectType.MetalEnergyCostReduction);

            case UpgradeCategory.Shuttle:
                return HasUnlockedBuildingEffect(
                    UpgradeEffectType.PlatformCapacity,
                    UpgradeEffectType.OrePerSecond);

            default:
                return false;
        }
    }

    public TemporaryBoostState GetNextAvailableTemporaryBoost()
    {
        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            TemporaryBoostState state = temporaryBoostStates[i];

            if (state.IsAvailable && !state.IsActive && IsTemporaryBoostTargetUnlocked(state))
            {
                return state;
            }
        }

        return null;
    }

    public IReadOnlyList<TemporaryBoostState> GetAvailableTemporaryBoosts()
    {
        List<TemporaryBoostState> availableBoostStates = new List<TemporaryBoostState>();

        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            TemporaryBoostState state = temporaryBoostStates[i];

            if (state.IsAvailable && !state.IsActive && IsTemporaryBoostTargetUnlocked(state))
            {
                availableBoostStates.Add(state);
            }
        }

        return availableBoostStates;
    }

    public void Update(float deltaTime)
    {
        AdvanceTime(deltaTime);
    }

    public void AdvanceTime(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        bool hasChanges = false;
        float remainingTime = deltaTime;

        while (remainingTime > 0f)
        {
            float stepTime = remainingTime;
            float nextActiveExpiration = GetNextActiveBoostExpirationSeconds();
            float nextAvailabilityTrigger = GetNextAvailabilityTriggerSeconds();

            if (nextActiveExpiration > 0f)
            {
                stepTime = Mathf.Min(stepTime, nextActiveExpiration);
            }

            if (nextAvailabilityTrigger > 0f)
            {
                stepTime = Mathf.Min(stepTime, nextAvailabilityTrigger);
            }

            if (stepTime <= 0f)
            {
                stepTime = remainingTime;
            }

            hasChanges = UpdateTemporaryBoostAvailability(stepTime) || hasChanges;
            hasChanges = UpdateActiveBoosts(stepTime) || hasChanges;
            remainingTime -= stepTime;
        }

        if (hasChanges)
        {
            UpgradesChanged?.Invoke();
        }
    }

    public void RecalculateIncome()
    {
        float orePerClick = GetBaseOrePerClick();
        float orePerSecond = GetBaseOrePerSecond();
        bool hasMiningPlatform = HasUnlockedBuildingEffect(
            UpgradeEffectType.PlatformCapacity,
            UpgradeEffectType.OrePerSecond);
        bool hasPowerStation = HasUnlockedBuildingEffect(
            UpgradeEffectType.EnergyCapacity,
            UpgradeEffectType.EnergyRegenAmount,
            UpgradeEffectType.EnergyRegenIntervalReduction);
        bool hasMetalFactory = HasUnlockedBuildingEffect(
            UpgradeEffectType.MetalProductionAmount,
            UpgradeEffectType.MetalOreCostReduction,
            UpgradeEffectType.MetalEnergyCostReduction);
        int energyMax = hasPowerStation ? GetBaseEnergyMax() : 0;
        int energyRegenAmount = hasPowerStation ? GetBaseEnergyRegenAmount() : 0;
        float energyRegenInterval = hasPowerStation ? GetBaseEnergyRegenInterval() : 0f;
        int metalPerCraft = hasMetalFactory ? GetBaseMetalPerCraft() : 0;
        int metalOreCost = hasMetalFactory ? GetBaseMetalOreCost() : 0;
        int metalEnergyCost = hasMetalFactory ? GetBaseMetalEnergyCost() : 0;
        int platformCapacity = GetBasePlatformCapacity();
        int shuttleCapacity = GetBaseShuttleCapacity();
        float shuttleLoadingTimeSeconds = GetBaseShuttleLoadingTimeSeconds();
        float shuttleTravelTimeSeconds = GetBaseShuttleTravelTimeSeconds();
        int shuttleCount = 1;
        int shuttleAutoSendCount = 0;
        bool legacyMetaAutoSendEnabled = false;

        ApplyEffects(buildingStates, ref orePerClick, ref orePerSecond, ref energyMax, ref energyRegenAmount,
            ref energyRegenInterval, ref metalPerCraft, ref metalOreCost, ref metalEnergyCost,
            ref platformCapacity, ref shuttleCapacity, ref shuttleLoadingTimeSeconds, ref shuttleTravelTimeSeconds, ref shuttleCount, ref shuttleAutoSendCount);

        ApplyEffects(upgradeStates, ref orePerClick, ref orePerSecond, ref energyMax, ref energyRegenAmount,
            ref energyRegenInterval, ref metalPerCraft, ref metalOreCost, ref metalEnergyCost,
            ref platformCapacity, ref shuttleCapacity, ref shuttleLoadingTimeSeconds, ref shuttleTravelTimeSeconds, ref shuttleCount, ref shuttleAutoSendCount);

        if (missionManager != null)
        {
            missionManager.ApplyMetaBonusEffects(
                ref orePerClick,
                ref orePerSecond,
                ref energyMax,
                ref energyRegenAmount,
                ref energyRegenInterval,
                ref metalPerCraft,
                ref metalOreCost,
                ref metalEnergyCost,
                ref platformCapacity,
                ref shuttleCapacity,
                ref shuttleLoadingTimeSeconds,
                ref shuttleTravelTimeSeconds,
                ref legacyMetaAutoSendEnabled);

            if (legacyMetaAutoSendEnabled)
            {
                shuttleAutoSendCount = Mathf.Max(shuttleAutoSendCount, 1);
            }
        }

        float orePerClickMultiplier = 1f;
        float orePerSecondMultiplier = 1f;
        float shuttleTravelSpeedMultiplier = 1f;

        for (int i = 0; i < activeTemporaryBoostStates.Count; i++)
        {
            TemporaryBoostState activeBoostState = activeTemporaryBoostStates[i];

            switch (activeBoostState.Definition.targetType)
            {
                case TemporaryBoostTargetType.OrePerClick:
                    orePerClickMultiplier *= activeBoostState.GetMultiplier();
                    break;

                case TemporaryBoostTargetType.OrePerSecond:
                    orePerSecondMultiplier *= activeBoostState.GetMultiplier();
                    break;

                case TemporaryBoostTargetType.ShuttleTravelSpeed:
                    shuttleTravelSpeedMultiplier *= activeBoostState.GetMultiplier();
                    break;
            }
        }

        gameData.orePerClick = Mathf.Max(GetBaseOrePerClick(), Mathf.RoundToInt(orePerClick * orePerClickMultiplier));
        gameData.orePerSecond = Mathf.Max(GetBaseOrePerSecond(), Mathf.RoundToInt(orePerSecond * orePerSecondMultiplier));
        gameData.energyMax = hasPowerStation ? Mathf.Max(1, energyMax) : 0;
        gameData.energyRegenAmount = hasPowerStation ? Mathf.Max(0, energyRegenAmount) : 0;
        gameData.energyRegenInterval = hasPowerStation
            ? Mathf.Max(GetMinEnergyRegenInterval(), energyRegenInterval)
            : 0f;
        gameData.metalPerCraft = hasMetalFactory ? Mathf.Max(1, metalPerCraft) : 0;
        gameData.metalOreCost = hasMetalFactory ? Mathf.Max(0, metalOreCost) : 0;
        gameData.metalEnergyCost = hasMetalFactory ? Mathf.Max(0, metalEnergyCost) : 0;
        gameData.hasMiningPlatform = hasMiningPlatform;
        gameData.platformCapacity = hasMiningPlatform ? Mathf.Max(1, platformCapacity) : 0;
        gameData.shuttleCapacity = Mathf.Max(1, shuttleCapacity);
        gameData.shuttleLoadingTimeSeconds = Mathf.Max(0f, shuttleLoadingTimeSeconds);
        gameData.shuttleTravelTimeSeconds = Mathf.Max(0f, shuttleTravelTimeSeconds / Mathf.Max(1f, shuttleTravelSpeedMultiplier));
        gameData.shuttleCount = Mathf.Clamp(shuttleCount, 1, GameData.MaxShuttles);
        gameData.shuttleAutoSendCount = Mathf.Clamp(shuttleAutoSendCount, 0, gameData.shuttleCount);
        gameData.shuttleOre = hasMiningPlatform
            ? Mathf.Clamp(gameData.shuttleOre, 0, gameData.platformCapacity)
            : Mathf.Clamp(gameData.shuttleOre, 0, gameData.shuttleCapacity);
        gameData.energy = hasPowerStation
            ? Mathf.Min(gameData.energy, gameData.energyMax)
            : 0;

        for (int i = 0; i < gameData.ActiveShuttleCount; i++)
        {
            ShuttleState shuttleState = gameData.GetShuttleState(i);
            shuttleState.dockedOre = hasMiningPlatform
                ? Mathf.Clamp(shuttleState.dockedOre, 0, gameData.shuttleCapacity)
                : 0;
            shuttleState.loadingOre = Mathf.Clamp(shuttleState.loadingOre, 0, gameData.shuttleCapacity);
            shuttleState.loadingTargetOre = Mathf.Max(shuttleState.loadingOre, shuttleState.loadingTargetOre);
            shuttleState.deliveringOre = Mathf.Clamp(shuttleState.deliveringOre, 0, gameData.shuttleCapacity);

            if (shuttleState.loadingCooldownRemaining > 0f)
            {
                shuttleState.loadingCooldownRemaining = Mathf.Min(
                    shuttleState.loadingCooldownRemaining,
                    gameData.shuttleLoadingTimeSeconds);
            }

            if (shuttleState.sendCooldownRemaining > 0f)
            {
                shuttleState.sendCooldownRemaining = Mathf.Min(
                    shuttleState.sendCooldownRemaining,
                    gameData.shuttleTravelTimeSeconds);
            }
        }

        gameData.ResetUnusedShuttles();

        if (gameData.shuttleLoadingCooldownRemaining > 0f)
        {
            gameData.shuttleLoadingCooldownRemaining = Mathf.Min(
                gameData.shuttleLoadingCooldownRemaining,
                gameData.shuttleLoadingTimeSeconds);
        }

        if (gameData.shuttleSendCooldownRemaining > 0f)
        {
            gameData.shuttleSendCooldownRemaining = Mathf.Min(
                gameData.shuttleSendCooldownRemaining,
                gameData.shuttleTravelTimeSeconds);
        }

        if (!hasPowerStation || gameData.energy >= gameData.energyMax)
        {
            gameData.energyRegenTimer = 0f;
        }
    }

    private void BuildUpgradeStates(UpgradeConfig upgradeConfig)
    {
        upgradeStates.Clear();

        if (upgradeConfig == null || upgradeConfig.Upgrades == null)
        {
            return;
        }

        for (int i = 0; i < upgradeConfig.Upgrades.Count; i++)
        {
            UpgradeDefinition definition = upgradeConfig.Upgrades[i];

            if (definition == null || string.IsNullOrEmpty(definition.id))
            {
                continue;
            }

            upgradeStates.Add(new UpgradeState(definition));
        }
    }

    private void BuildBuildingStates(BuildingConfig buildingConfig)
    {
        buildingStates.Clear();

        if (buildingConfig == null || buildingConfig.Buildings == null)
        {
            return;
        }

        for (int i = 0; i < buildingConfig.Buildings.Count; i++)
        {
            BuildingDefinition definition = buildingConfig.Buildings[i];

            if (definition == null || string.IsNullOrEmpty(definition.id))
            {
                continue;
            }

            buildingStates.Add(new BuildingState(definition));
        }
    }

    private void BuildTemporaryBoostStates(TemporaryBoostConfig temporaryBoostConfig)
    {
        temporaryBoostStates.Clear();

        if (temporaryBoostConfig == null || temporaryBoostConfig.TemporaryBoosts == null)
        {
            return;
        }

        HashSet<string> usedIds = new HashSet<string>();

        for (int i = 0; i < temporaryBoostConfig.TemporaryBoosts.Count; i++)
        {
            TemporaryBoostDefinition definition = temporaryBoostConfig.TemporaryBoosts[i];

            if (definition == null || string.IsNullOrEmpty(definition.id))
            {
                continue;
            }

            if (!usedIds.Add(definition.id))
            {
                Debug.LogWarning("Duplicate temporary boost id found and skipped: " + definition.id);
                continue;
            }

            temporaryBoostStates.Add(new TemporaryBoostState(definition));
        }
    }

    private void ActivateBoost(TemporaryBoostState state)
    {
        state.Activate(state.Definition.durationSeconds);
        activeTemporaryBoostStates.Add(state);
    }

    private void InitializeTemporaryBoostState(TemporaryBoostState state)
    {
        state.Deactivate();
        state.SetAvailable(false);
        state.SetTimeUntilAvailable(state.Definition.appearanceIntervalSeconds);
        state.SetNextOreThreshold(state.Definition.oreRequiredForAppearance);

        if (state.Definition.availabilityType == TemporaryBoostAvailabilityType.ByAccumulatedOre)
        {
            state.SetAvailable(
                gameData.totalOreEarned >= state.NextOreThreshold &&
                IsTemporaryBoostTargetUnlocked(state));
        }
    }

    private void LoadTemporaryBoostState(TemporaryBoostState state)
    {
        string availableKey = GetTemporaryBoostAvailableKey(state.Definition.id);
        string timeKey = GetTemporaryBoostTimeKey(state.Definition.id);
        string oreThresholdKey = GetTemporaryBoostOreThresholdKey(state.Definition.id);
        string activeTimeKey = GetTemporaryBoostActiveTimeKey(state.Definition.id);

        bool hasSavedState =
            PlayerPrefs.HasKey(availableKey) ||
            PlayerPrefs.HasKey(timeKey) ||
            PlayerPrefs.HasKey(oreThresholdKey) ||
            PlayerPrefs.HasKey(activeTimeKey);

        if (!hasSavedState)
        {
            InitializeTemporaryBoostState(state);
            return;
        }

        state.SetAvailable(PlayerPrefs.GetInt(availableKey, 0) == 1);
        state.SetTimeUntilAvailable(PlayerPrefs.GetFloat(timeKey, state.Definition.appearanceIntervalSeconds));
        state.SetNextOreThreshold(PlayerPrefs.GetInt(oreThresholdKey, state.Definition.oreRequiredForAppearance));
        state.Deactivate();
        float savedActiveRemainingTime = Mathf.Max(0f, PlayerPrefs.GetFloat(activeTimeKey, 0f));

        if (savedActiveRemainingTime > 0f)
        {
            state.Activate(savedActiveRemainingTime);
            state.SetAvailable(false);
            return;
        }

        if (state.IsAvailable && !IsTemporaryBoostTargetUnlocked(state))
        {
            state.SetAvailable(false);
        }

        if (state.IsAvailable)
        {
            return;
        }

        if (state.Definition.availabilityType == TemporaryBoostAvailabilityType.ByTime &&
            state.TimeUntilAvailable <= 0f &&
            IsTemporaryBoostTargetUnlocked(state))
        {
            state.SetAvailable(true);
            return;
        }

        if (state.Definition.availabilityType == TemporaryBoostAvailabilityType.ByAccumulatedOre &&
            gameData.totalOreEarned >= state.NextOreThreshold &&
            IsTemporaryBoostTargetUnlocked(state))
        {
            state.SetAvailable(true);
        }
    }

    private void SaveTemporaryBoostState(TemporaryBoostState state)
    {
        PlayerPrefs.SetInt(GetTemporaryBoostAvailableKey(state.Definition.id), state.IsAvailable ? 1 : 0);
        PlayerPrefs.SetFloat(GetTemporaryBoostTimeKey(state.Definition.id), state.TimeUntilAvailable);
        PlayerPrefs.SetInt(GetTemporaryBoostOreThresholdKey(state.Definition.id), state.NextOreThreshold);
        PlayerPrefs.SetFloat(GetTemporaryBoostActiveTimeKey(state.Definition.id), state.ActiveRemainingTime);
    }

    private void ResetTemporaryBoostAvailability(TemporaryBoostState state)
    {
        state.SetAvailable(false);

        if (state.Definition.availabilityType == TemporaryBoostAvailabilityType.ByTime)
        {
            state.SetTimeUntilAvailable(state.Definition.appearanceIntervalSeconds);
            return;
        }

        state.SetNextOreThreshold(gameData.totalOreEarned + state.Definition.oreRequiredForAppearance);
    }

    private void DeleteTemporaryBoostState(TemporaryBoostState state)
    {
        PlayerPrefs.DeleteKey(GetTemporaryBoostAvailableKey(state.Definition.id));
        PlayerPrefs.DeleteKey(GetTemporaryBoostTimeKey(state.Definition.id));
        PlayerPrefs.DeleteKey(GetTemporaryBoostOreThresholdKey(state.Definition.id));
        PlayerPrefs.DeleteKey(GetTemporaryBoostActiveTimeKey(state.Definition.id));
    }

    private bool UpdateTemporaryBoostAvailability(float deltaTime)
    {
        bool hasChanges = false;

        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            TemporaryBoostState state = temporaryBoostStates[i];

            if (state.IsAvailable || state.IsActive)
            {
                continue;
            }

            switch (state.Definition.availabilityType)
            {
                case TemporaryBoostAvailabilityType.ByTime:
                    state.SetTimeUntilAvailable(state.TimeUntilAvailable - deltaTime);

                    if (state.TimeUntilAvailable <= 0f && IsTemporaryBoostTargetUnlocked(state))
                    {
                        state.SetAvailable(true);
                        hasChanges = true;
                    }
                    break;

                case TemporaryBoostAvailabilityType.ByAccumulatedOre:
                    if (gameData.totalOreEarned >= state.NextOreThreshold &&
                        IsTemporaryBoostTargetUnlocked(state))
                    {
                        state.SetAvailable(true);
                        hasChanges = true;
                    }
                    break;
            }
        }

        return hasChanges;
    }

    private bool UpdateActiveBoosts(float deltaTime)
    {
        bool hasChanges = false;

        for (int i = activeTemporaryBoostStates.Count - 1; i >= 0; i--)
        {
            TemporaryBoostState state = activeTemporaryBoostStates[i];
            state.SetActiveRemainingTime(state.ActiveRemainingTime - deltaTime);

            if (state.ActiveRemainingTime > 0f)
            {
                continue;
            }

            state.Deactivate();
            activeTemporaryBoostStates.RemoveAt(i);
            hasChanges = true;
        }

        if (hasChanges)
        {
            RecalculateIncome();
        }

        return hasChanges;
    }

    private string GetUpgradeLevelKey(string upgradeId)
    {
        return UpgradeLevelKeyPrefix + upgradeId;
    }

    private string GetBuildingLevelKey(string buildingId)
    {
        return BuildingLevelKeyPrefix + buildingId;
    }

    private string GetTemporaryBoostAvailableKey(string boostId)
    {
        return TemporaryBoostAvailableKeyPrefix + boostId;
    }

    private string GetTemporaryBoostTimeKey(string boostId)
    {
        return TemporaryBoostTimeKeyPrefix + boostId;
    }

    private string GetTemporaryBoostOreThresholdKey(string boostId)
    {
        return TemporaryBoostOreThresholdKeyPrefix + boostId;
    }

    private string GetTemporaryBoostActiveTimeKey(string boostId)
    {
        return TemporaryBoostActiveTimeKeyPrefix + boostId;
    }

    private int GetBaseShuttleCapacity()
    {
        return gameConfig != null
            ? gameConfig.Capacity
            : ShuttleConfig.DefaultCapacity;
    }

    private float GetBaseShuttleLoadingTimeSeconds()
    {
        return gameConfig != null
            ? gameConfig.LoadingTimeSeconds
            : ShuttleConfig.DefaultLoadingTimeSeconds;
    }

    private float GetBaseShuttleTravelTimeSeconds()
    {
        return gameConfig != null
            ? gameConfig.TravelTimeSeconds
            : ShuttleConfig.DefaultTravelTimeSeconds;
    }

    private int GetBaseOrePerClick()
    {
        return gameConfig != null
            ? gameConfig.StartOrePerClick
            : ShuttleConfig.DefaultStartOrePerClick;
    }

    private int GetBaseOrePerSecond()
    {
        return gameConfig != null
            ? gameConfig.StartOrePerSecond
            : ShuttleConfig.DefaultStartOrePerSecond;
    }

    private int GetBaseEnergyMax()
    {
        return gameConfig != null
            ? gameConfig.StartEnergyMax
            : ShuttleConfig.DefaultStartEnergyMax;
    }

    private int GetBaseEnergyRegenAmount()
    {
        return gameConfig != null
            ? gameConfig.StartEnergyRegenAmount
            : ShuttleConfig.DefaultStartEnergyRegenAmount;
    }

    private float GetBaseEnergyRegenInterval()
    {
        return gameConfig != null
            ? gameConfig.StartEnergyRegenInterval
            : ShuttleConfig.DefaultStartEnergyRegenInterval;
    }

    private float GetMinEnergyRegenInterval()
    {
        return gameConfig != null
            ? gameConfig.MinEnergyRegenInterval
            : ShuttleConfig.DefaultMinEnergyRegenInterval;
    }

    private int GetBaseMetalPerCraft()
    {
        return gameConfig != null
            ? gameConfig.MetalPerCraft
            : ShuttleConfig.DefaultMetalPerCraft;
    }

    private int GetBaseMetalOreCost()
    {
        return gameConfig != null
            ? gameConfig.MetalOreCost
            : ShuttleConfig.DefaultMetalOreCost;
    }

    private int GetBaseMetalEnergyCost()
    {
        return gameConfig != null
            ? gameConfig.MetalEnergyCost
            : ShuttleConfig.DefaultMetalEnergyCost;
    }

    private int GetBasePlatformCapacity()
    {
        return gameConfig != null
            ? gameConfig.StartPlatformCapacity
            : ShuttleConfig.DefaultPlatformCapacity;
    }

    private bool HasUnlockedBuildingEffect(params UpgradeEffectType[] effectTypes)
    {
        if (effectTypes == null || effectTypes.Length <= 0)
        {
            return false;
        }

        for (int buildingIndex = 0; buildingIndex < buildingStates.Count; buildingIndex++)
        {
            BuildingState buildingState = buildingStates[buildingIndex];

            if (buildingState.Level <= 0)
            {
                continue;
            }

            IReadOnlyList<UpgradeEffectDefinition> effects = buildingState.Definition.Effects;

            for (int effectIndex = 0; effectIndex < effects.Count; effectIndex++)
            {
                UpgradeEffectType effectType = effects[effectIndex].effectType;

                for (int requestedIndex = 0; requestedIndex < effectTypes.Length; requestedIndex++)
                {
                    if (effectType == effectTypes[requestedIndex])
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsTemporaryBoostTargetUnlocked(TemporaryBoostState state)
    {
        if (state == null || state.Definition == null)
        {
            return false;
        }

        switch (state.Definition.targetType)
        {
            case TemporaryBoostTargetType.OrePerClick:
                return gameData.orePerClick > 0;

            case TemporaryBoostTargetType.OrePerSecond:
                return gameData.orePerSecond > 0;

            case TemporaryBoostTargetType.ShuttleTravelSpeed:
                return gameData.shuttleTravelTimeSeconds > 0f;

            default:
                return true;
        }
    }

    private float GetNextActiveBoostExpirationSeconds()
    {
        float nextExpirationSeconds = 0f;

        for (int i = 0; i < activeTemporaryBoostStates.Count; i++)
        {
            float remainingTime = activeTemporaryBoostStates[i].ActiveRemainingTime;

            if (remainingTime <= 0f)
            {
                continue;
            }

            if (nextExpirationSeconds <= 0f || remainingTime < nextExpirationSeconds)
            {
                nextExpirationSeconds = remainingTime;
            }
        }

        return nextExpirationSeconds;
    }

    private float GetNextAvailabilityTriggerSeconds()
    {
        float nextAvailabilitySeconds = 0f;

        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            TemporaryBoostState state = temporaryBoostStates[i];

            if (state.IsAvailable ||
                state.IsActive ||
                state.Definition.availabilityType != TemporaryBoostAvailabilityType.ByTime ||
                state.TimeUntilAvailable <= 0f)
            {
                continue;
            }

            if (nextAvailabilitySeconds <= 0f || state.TimeUntilAvailable < nextAvailabilitySeconds)
            {
                nextAvailabilitySeconds = state.TimeUntilAvailable;
            }
        }

        return nextAvailabilitySeconds;
    }

    private void ApplyEffects(
        IReadOnlyList<UpgradeState> states,
        ref float orePerClick,
        ref float orePerSecond,
        ref int energyMax,
        ref int energyRegenAmount,
        ref float energyRegenInterval,
        ref int metalPerCraft,
        ref int metalOreCost,
        ref int metalEnergyCost,
        ref int platformCapacity,
        ref int shuttleCapacity,
        ref float shuttleLoadingTimeSeconds,
        ref float shuttleTravelTimeSeconds,
        ref int shuttleCount,
        ref int shuttleAutoSendCount)
    {
        for (int i = 0; i < states.Count; i++)
        {
            UpgradeState state = states[i];

            if (state.Level <= 0)
            {
                continue;
            }

            ApplyEffectList(
                state.Definition.Effects,
                state.GetCurrentEffectValue,
                ref orePerClick,
                ref orePerSecond,
                ref energyMax,
                ref energyRegenAmount,
                ref energyRegenInterval,
                ref metalPerCraft,
                ref metalOreCost,
                ref metalEnergyCost,
                ref platformCapacity,
                ref shuttleCapacity,
                ref shuttleLoadingTimeSeconds,
                ref shuttleTravelTimeSeconds,
                ref shuttleCount,
                ref shuttleAutoSendCount);
        }
    }

    private void ApplyEffects(
        IReadOnlyList<BuildingState> states,
        ref float orePerClick,
        ref float orePerSecond,
        ref int energyMax,
        ref int energyRegenAmount,
        ref float energyRegenInterval,
        ref int metalPerCraft,
        ref int metalOreCost,
        ref int metalEnergyCost,
        ref int platformCapacity,
        ref int shuttleCapacity,
        ref float shuttleLoadingTimeSeconds,
        ref float shuttleTravelTimeSeconds,
        ref int shuttleCount,
        ref int shuttleAutoSendCount)
    {
        for (int i = 0; i < states.Count; i++)
        {
            BuildingState state = states[i];

            if (state.Level <= 0)
            {
                continue;
            }

            ApplyEffectList(
                state.Definition.Effects,
                state.GetCurrentEffectValue,
                ref orePerClick,
                ref orePerSecond,
                ref energyMax,
                ref energyRegenAmount,
                ref energyRegenInterval,
                ref metalPerCraft,
                ref metalOreCost,
                ref metalEnergyCost,
                ref platformCapacity,
                ref shuttleCapacity,
                ref shuttleLoadingTimeSeconds,
                ref shuttleTravelTimeSeconds,
                ref shuttleCount,
                ref shuttleAutoSendCount);
        }
    }

    private void ApplyEffectList(
        IReadOnlyList<UpgradeEffectDefinition> effects,
        Func<UpgradeEffectDefinition, float> getEffectValue,
        ref float orePerClick,
        ref float orePerSecond,
        ref int energyMax,
        ref int energyRegenAmount,
        ref float energyRegenInterval,
        ref int metalPerCraft,
        ref int metalOreCost,
        ref int metalEnergyCost,
        ref int platformCapacity,
        ref int shuttleCapacity,
        ref float shuttleLoadingTimeSeconds,
        ref float shuttleTravelTimeSeconds,
        ref int shuttleCount,
        ref int shuttleAutoSendCount)
    {
        for (int effectIndex = 0; effectIndex < effects.Count; effectIndex++)
        {
            UpgradeEffectDefinition effect = effects[effectIndex];
            float effectValue = getEffectValue(effect);

            switch (effect.effectType)
            {
                case UpgradeEffectType.OrePerClick:
                    orePerClick += effectValue;
                    break;

                case UpgradeEffectType.OrePerSecond:
                    orePerSecond += effectValue;
                    break;

                case UpgradeEffectType.EnergyCapacity:
                    energyMax += Mathf.RoundToInt(effectValue);
                    break;

                case UpgradeEffectType.EnergyRegenAmount:
                    energyRegenAmount += Mathf.RoundToInt(effectValue);
                    break;

                case UpgradeEffectType.EnergyRegenIntervalReduction:
                    energyRegenInterval -= effectValue;
                    break;

                case UpgradeEffectType.MetalProductionAmount:
                    metalPerCraft += Mathf.RoundToInt(effectValue);
                    break;

                case UpgradeEffectType.MetalOreCostReduction:
                    metalOreCost -= Mathf.RoundToInt(effectValue);
                    break;

                case UpgradeEffectType.MetalEnergyCostReduction:
                    metalEnergyCost -= Mathf.RoundToInt(effectValue);
                    break;

                case UpgradeEffectType.PlatformCapacity:
                    platformCapacity += Mathf.RoundToInt(effectValue);
                    break;

                case UpgradeEffectType.ShuttleCapacity:
                    shuttleCapacity += Mathf.RoundToInt(effectValue);
                    break;

                case UpgradeEffectType.ShuttleLoadingTimeReduction:
                    shuttleLoadingTimeSeconds -= effectValue;
                    break;

                case UpgradeEffectType.ShuttleTravelTimeReduction:
                    shuttleTravelTimeSeconds -= effectValue;
                    break;

                case UpgradeEffectType.ShuttleAutoSend:
                    shuttleAutoSendCount += Mathf.Max(1, Mathf.RoundToInt(effectValue));
                    break;

                case UpgradeEffectType.ShuttleCount:
                    shuttleCount += Mathf.RoundToInt(effectValue);
                    break;
            }
        }
    }
}
