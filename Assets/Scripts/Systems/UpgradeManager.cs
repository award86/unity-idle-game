using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager
{
    private const string UpgradeLevelKeyPrefix = "upgrade_level_";

    private readonly GameData gameData;
    private readonly List<UpgradeState> upgradeStates = new List<UpgradeState>();

    private UpgradeState activeBoostState;
    private float activeBoostRemainingTime;

    public event Action UpgradesChanged;

    public UpgradeManager(GameData gameData, UpgradeConfig upgradeConfig)
    {
        this.gameData = gameData;
        BuildUpgradeStates(upgradeConfig);
        RecalculateIncome();
    }

    public IReadOnlyList<UpgradeState> UpgradeStates => upgradeStates;
    public bool HasActiveBoost => activeBoostState != null && activeBoostRemainingTime > 0f;
    public string ActiveBoostName => HasActiveBoost ? activeBoostState.Definition.upgradeName : string.Empty;
    public float ActiveBoostMultiplier => HasActiveBoost ? activeBoostState.GetCurrentEffectValue() : 1f;
    public float ActiveBoostRemainingTime => activeBoostRemainingTime;

    public void LoadUpgradeLevels()
    {
        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];
            int savedLevel = Mathf.Max(0, PlayerPrefs.GetInt(GetUpgradeLevelKey(state.Definition.id), 0));
            state.SetLevel(savedLevel);
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
    }

    public void ResetUpgrades()
    {
        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeState state = upgradeStates[i];
            state.SetLevel(0);
            PlayerPrefs.DeleteKey(GetUpgradeLevelKey(state.Definition.id));
        }

        activeBoostState = null;
        activeBoostRemainingTime = 0f;

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

        if (state.Definition.IsTemporaryBoost)
        {
            ActivateBoost(state);
        }

        RecalculateIncome();
        UpgradesChanged?.Invoke();
        return true;
    }

    public void Update(float deltaTime)
    {
        if (!HasActiveBoost)
        {
            return;
        }

        activeBoostRemainingTime -= deltaTime;

        if (activeBoostRemainingTime > 0f)
        {
            return;
        }

        activeBoostRemainingTime = 0f;
        activeBoostState = null;
        RecalculateIncome();
        UpgradesChanged?.Invoke();
    }

    public void RecalculateIncome()
    {
        float orePerClick = GameSettings.StartOrePerClick;
        float orePerSecond = GameSettings.StartOrePerSecond;
        float orePerClickMultiplier = 1f;
        float orePerSecondMultiplier = 1f;

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
                case UpgradeEffectType.OrePerClickFlat:
                    orePerClick += effectValue;
                    break;

                case UpgradeEffectType.OrePerClickMultiplier:
                    orePerClickMultiplier *= 1f + effectValue;
                    break;

                case UpgradeEffectType.OrePerSecondFlat:
                    orePerSecond += effectValue;
                    break;

                case UpgradeEffectType.OrePerSecondMultiplier:
                    orePerSecondMultiplier *= 1f + effectValue;
                    break;
            }
        }

        float boostMultiplier = HasActiveBoost ? ActiveBoostMultiplier : 1f;

        gameData.orePerClick = Mathf.Max(GameSettings.StartOrePerClick, Mathf.RoundToInt(orePerClick * orePerClickMultiplier * boostMultiplier));
        gameData.orePerSecond = Mathf.Max(GameSettings.StartOrePerSecond, Mathf.RoundToInt(orePerSecond * orePerSecondMultiplier * boostMultiplier));
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

    private void ActivateBoost(UpgradeState state)
    {
        activeBoostState = state;
        activeBoostRemainingTime = Mathf.Max(0f, state.Definition.durationSeconds);
    }

    private string GetUpgradeLevelKey(string upgradeId)
    {
        return UpgradeLevelKeyPrefix + upgradeId;
    }
}
