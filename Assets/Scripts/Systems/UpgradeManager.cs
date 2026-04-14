using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager
{
    private const string UpgradeLevelKeyPrefix = "upgrade_level_";
    private const string TemporaryBoostAvailableKeyPrefix = "temporary_boost_available_";
    private const string TemporaryBoostTimeKeyPrefix = "temporary_boost_time_";
    private const string TemporaryBoostOreThresholdKeyPrefix = "temporary_boost_ore_threshold_";

    private readonly GameData gameData;
    private readonly List<UpgradeState> upgradeStates = new List<UpgradeState>();
    private readonly List<TemporaryBoostState> temporaryBoostStates = new List<TemporaryBoostState>();
    private readonly List<TemporaryBoostState> activeTemporaryBoostStates = new List<TemporaryBoostState>();

    public event Action UpgradesChanged;

    public UpgradeManager(GameData gameData, UpgradeConfig upgradeConfig, TemporaryBoostConfig temporaryBoostConfig)
    {
        this.gameData = gameData;
        BuildUpgradeStates(upgradeConfig);
        BuildTemporaryBoostStates(temporaryBoostConfig);
        RecalculateIncome();
    }

    public IReadOnlyList<UpgradeState> UpgradeStates => upgradeStates;
    public IReadOnlyList<TemporaryBoostState> TemporaryBoostStates => temporaryBoostStates;
    public IReadOnlyList<TemporaryBoostState> ActiveTemporaryBoostStates => activeTemporaryBoostStates;
    public bool HasActiveBoost => activeTemporaryBoostStates.Count > 0;

    public void LoadUpgradeLevels()
    {
        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];
            int savedLevel = Mathf.Max(0, PlayerPrefs.GetInt(GetUpgradeLevelKey(state.Definition.id), 0));
            state.SetLevel(savedLevel);
        }

        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            LoadTemporaryBoostState(temporaryBoostStates[i]);
        }

        activeTemporaryBoostStates.Clear();
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

    public bool TryBuyUpgrade(UpgradeState state)
    {
        if (state == null || state.IsMaxLevel)
        {
            return false;
        }

        int cost = state.GetCurrentCost();

        if (gameData.ore < cost)
        {
            return false;
        }

        gameData.ore -= cost;
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

    public bool HasAffordableUpgrade(int currentOre)
    {
        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];

            if (state.IsMaxLevel)
            {
                continue;
            }

            if (currentOre >= state.GetCurrentCost())
            {
                return true;
            }
        }

        if (activeTemporaryBoostStates.Count >= GameSettings.MaxActiveTemporaryBoosts)
        {
            return false;
        }

        for (int i = 0; i < temporaryBoostStates.Count; i++)
        {
            TemporaryBoostState state = temporaryBoostStates[i];

            if (state.IsAvailable && !state.IsActive)
            {
                return true;
            }
        }

        return false;
    }

    public void Update(float deltaTime)
    {
        bool hasChanges = false;

        hasChanges = UpdateTemporaryBoostAvailability(deltaTime) || hasChanges;
        hasChanges = UpdateActiveBoosts(deltaTime) || hasChanges;

        if (hasChanges)
        {
            UpgradesChanged?.Invoke();
        }
    }

    public void RecalculateIncome()
    {
        float orePerClick = GameSettings.StartOrePerClick;
        float orePerSecond = GameSettings.StartOrePerSecond;
        float shuttleOrePerSecondMultiplier = 1f;

        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];
            float effectValue = state.GetCurrentEffectValue();

            if (effectValue <= 0f)
            {
                continue;
            }

            switch (state.Definition.effectType)
            {
                case UpgradeEffectType.MiningPerClick:
                    orePerClick += effectValue;
                    break;

                case UpgradeEffectType.MiningPerSecond:
                    orePerSecond += effectValue;
                    break;

                case UpgradeEffectType.Shuttle:
                    shuttleOrePerSecondMultiplier *= 1f + effectValue;
                    break;
            }
        }

        float orePerClickMultiplier = 1f;
        float orePerSecondMultiplier = 1f;

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
            }
        }

        gameData.orePerClick = Mathf.Max(GameSettings.StartOrePerClick, Mathf.RoundToInt(orePerClick * orePerClickMultiplier));
        gameData.orePerSecond = Mathf.Max(GameSettings.StartOrePerSecond, Mathf.RoundToInt(orePerSecond * shuttleOrePerSecondMultiplier * orePerSecondMultiplier));
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

    private void BuildTemporaryBoostStates(TemporaryBoostConfig temporaryBoostConfig)
    {
        temporaryBoostStates.Clear();

        if (temporaryBoostConfig == null || temporaryBoostConfig.TemporaryBoosts == null)
        {
            return;
        }

        for (int i = 0; i < temporaryBoostConfig.TemporaryBoosts.Count; i++)
        {
            TemporaryBoostDefinition definition = temporaryBoostConfig.TemporaryBoosts[i];

            if (definition == null || string.IsNullOrEmpty(definition.id))
            {
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
            state.SetAvailable(gameData.totalOreEarned >= state.NextOreThreshold);
        }
    }

    private void LoadTemporaryBoostState(TemporaryBoostState state)
    {
        string availableKey = GetTemporaryBoostAvailableKey(state.Definition.id);
        string timeKey = GetTemporaryBoostTimeKey(state.Definition.id);
        string oreThresholdKey = GetTemporaryBoostOreThresholdKey(state.Definition.id);

        bool hasSavedState =
            PlayerPrefs.HasKey(availableKey) ||
            PlayerPrefs.HasKey(timeKey) ||
            PlayerPrefs.HasKey(oreThresholdKey);

        if (!hasSavedState)
        {
            InitializeTemporaryBoostState(state);
            return;
        }

        state.SetAvailable(PlayerPrefs.GetInt(availableKey, 0) == 1);
        state.SetTimeUntilAvailable(PlayerPrefs.GetFloat(timeKey, state.Definition.appearanceIntervalSeconds));
        state.SetNextOreThreshold(PlayerPrefs.GetInt(oreThresholdKey, state.Definition.oreRequiredForAppearance));
        state.Deactivate();

        if (state.IsAvailable)
        {
            return;
        }

        if (state.Definition.availabilityType == TemporaryBoostAvailabilityType.ByTime &&
            state.TimeUntilAvailable <= 0f)
        {
            state.SetAvailable(true);
            return;
        }

        if (state.Definition.availabilityType == TemporaryBoostAvailabilityType.ByAccumulatedOre &&
            gameData.totalOreEarned >= state.NextOreThreshold)
        {
            state.SetAvailable(true);
        }
    }

    private void SaveTemporaryBoostState(TemporaryBoostState state)
    {
        PlayerPrefs.SetInt(GetTemporaryBoostAvailableKey(state.Definition.id), state.IsAvailable ? 1 : 0);
        PlayerPrefs.SetFloat(GetTemporaryBoostTimeKey(state.Definition.id), state.TimeUntilAvailable);
        PlayerPrefs.SetInt(GetTemporaryBoostOreThresholdKey(state.Definition.id), state.NextOreThreshold);
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

                    if (state.TimeUntilAvailable <= 0f)
                    {
                        state.SetAvailable(true);
                        hasChanges = true;
                    }
                    break;

                case TemporaryBoostAvailabilityType.ByAccumulatedOre:
                    if (gameData.totalOreEarned >= state.NextOreThreshold)
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
}
