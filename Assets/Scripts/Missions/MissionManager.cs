using System.Collections.Generic;
using UnityEngine;

public class MissionManager
{
    private const string MissionIndexKey = "mission_index";
    private const string MissionRewardReadyKey = "mission_reward_ready";
    private const string MetaBonusLevelKeyPrefix = "meta_bonus_level_";

    private readonly GameData gameData;
    private readonly List<MissionDefinition> missions = new List<MissionDefinition>();
    private readonly List<MetaBonusState> metaBonusStates = new List<MetaBonusState>();

    public MissionManager(GameData gameData, MissionConfig missionConfig, MetaBonusConfig metaBonusConfig)
    {
        this.gameData = gameData;
        BuildMissions(missionConfig);
        BuildMetaBonusStates(metaBonusConfig);
    }

    public IReadOnlyList<MetaBonusState> MetaBonusStates => metaBonusStates;
    public MissionDefinition ActiveMission => CurrentMissionIndex >= 0 && CurrentMissionIndex < missions.Count
        ? missions[CurrentMissionIndex]
        : null;
    public int CurrentMissionIndex { get; private set; }
    public bool HasActiveMission => ActiveMission != null;
    public bool IsActiveMissionReadyToClaim { get; private set; }

    public void LoadProgress()
    {
        CurrentMissionIndex = Mathf.Clamp(PlayerPrefs.GetInt(MissionIndexKey, 0), 0, missions.Count);
        IsActiveMissionReadyToClaim = PlayerPrefs.GetInt(MissionRewardReadyKey, 0) == 1 && HasActiveMission;

        if (IsActiveMissionReadyToClaim && CurrentMissionIndex == missions.Count - 1)
        {
            TryClaimActiveMissionReward();
        }

        for (int i = 0; i < metaBonusStates.Count; i++)
        {
            MetaBonusState state = metaBonusStates[i];
            state.SetLevel(Mathf.Max(0, PlayerPrefs.GetInt(GetMetaBonusLevelKey(state.Definition.id), 0)));
        }
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(MissionIndexKey, CurrentMissionIndex);
        PlayerPrefs.SetInt(MissionRewardReadyKey, IsActiveMissionReadyToClaim ? 1 : 0);

        for (int i = 0; i < metaBonusStates.Count; i++)
        {
            MetaBonusState state = metaBonusStates[i];
            PlayerPrefs.SetInt(GetMetaBonusLevelKey(state.Definition.id), state.Level);
        }
    }

    public void ResetProgress()
    {
        CurrentMissionIndex = 0;
        IsActiveMissionReadyToClaim = false;
        PlayerPrefs.DeleteKey(MissionIndexKey);
        PlayerPrefs.DeleteKey(MissionRewardReadyKey);

        for (int i = 0; i < metaBonusStates.Count; i++)
        {
            MetaBonusState state = metaBonusStates[i];
            state.SetLevel(0);
            PlayerPrefs.DeleteKey(GetMetaBonusLevelKey(state.Definition.id));
        }
    }

    public bool TryBuyMetaBonus(MetaBonusState state)
    {
        if (state == null || state.IsMaxLevel || !state.CanAfford(gameData))
        {
            return false;
        }

        gameData.crystal -= state.GetCurrentCrystalCost();
        state.IncreaseLevel();
        return true;
    }

    public bool UpdateMissionProgress(UpgradeManager upgradeManager)
    {
        MissionDefinition activeMission = ActiveMission;

        if (activeMission == null || upgradeManager == null || IsActiveMissionReadyToClaim)
        {
            return false;
        }

        MissionProgressData progressData = BuildMissionProgressData(activeMission, upgradeManager);

        if (progressData.currentValue < progressData.targetValue)
        {
            return false;
        }

        if (CurrentMissionIndex >= missions.Count - 1)
        {
            gameData.crystal += Mathf.Max(0, activeMission.crystalReward);
            CurrentMissionIndex = missions.Count;
            IsActiveMissionReadyToClaim = false;
            return true;
        }

        IsActiveMissionReadyToClaim = true;
        return true;
    }

    public bool TryClaimActiveMissionReward()
    {
        MissionDefinition activeMission = ActiveMission;

        if (activeMission == null || !IsActiveMissionReadyToClaim)
        {
            return false;
        }

        gameData.crystal += Mathf.Max(0, activeMission.crystalReward);
        CurrentMissionIndex = Mathf.Min(CurrentMissionIndex + 1, missions.Count);
        IsActiveMissionReadyToClaim = false;
        return true;
    }

    public MissionProgressData GetMissionProgress(UpgradeManager upgradeManager)
    {
        MissionDefinition activeMission = ActiveMission;

        if (activeMission == null || upgradeManager == null)
        {
            return new MissionProgressData
            {
                hasMission = false,
                isCompleted = true,
                canClaimReward = false,
                missionName = string.Empty,
                description = string.Empty,
                progressLabel = string.Empty,
                currentValue = 0,
                targetValue = 0,
                crystalReward = 0,
                missionNumber = missions.Count,
                missionCount = missions.Count
            };
        }

        return BuildMissionProgressData(activeMission, upgradeManager);
    }

    public void ApplyMetaBonusEffects(
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
        ref bool shuttleAutoSendEnabled)
    {
        for (int i = 0; i < metaBonusStates.Count; i++)
        {
            MetaBonusState state = metaBonusStates[i];

            if (state.Level <= 0)
            {
                continue;
            }

            IReadOnlyList<UpgradeEffectDefinition> effects = state.Definition.Effects;

            for (int effectIndex = 0; effectIndex < effects.Count; effectIndex++)
            {
                UpgradeEffectDefinition effect = effects[effectIndex];
                float effectValue = state.GetCurrentEffectValue(effect);

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
                        shuttleAutoSendEnabled = true;
                        break;
                }
            }
        }
    }

    private void BuildMissions(MissionConfig missionConfig)
    {
        missions.Clear();

        if (missionConfig == null || missionConfig.Missions == null)
        {
            return;
        }

        for (int i = 0; i < missionConfig.Missions.Count; i++)
        {
            MissionDefinition definition = missionConfig.Missions[i];

            if (definition == null || string.IsNullOrEmpty(definition.id))
            {
                continue;
            }

            missions.Add(definition);
        }
    }

    private void BuildMetaBonusStates(MetaBonusConfig metaBonusConfig)
    {
        metaBonusStates.Clear();

        if (metaBonusConfig == null || metaBonusConfig.Bonuses == null)
        {
            return;
        }

        for (int i = 0; i < metaBonusConfig.Bonuses.Count; i++)
        {
            MetaBonusDefinition definition = metaBonusConfig.Bonuses[i];

            if (definition == null || string.IsNullOrEmpty(definition.id))
            {
                continue;
            }

            metaBonusStates.Add(new MetaBonusState(definition));
        }
    }

    private MissionProgressData BuildMissionProgressData(MissionDefinition mission, UpgradeManager upgradeManager)
    {
        MissionProgressData progressData = new MissionProgressData
        {
            hasMission = true,
            isCompleted = IsActiveMissionReadyToClaim,
            canClaimReward = IsActiveMissionReadyToClaim,
            missionName = mission.DisplayName,
            description = mission.Description,
            crystalReward = Mathf.Max(0, mission.crystalReward),
            missionNumber = CurrentMissionIndex + 1,
            missionCount = missions.Count
        };

        switch (mission.objectiveType)
        {
            case MissionObjectiveType.ReachOreAmount:
                progressData.progressLabel = GameTextProvider.UIText.OreLabel;
                progressData.currentValue = Mathf.Max(0, gameData.ore);
                progressData.targetValue = Mathf.Max(1, mission.targetValue);
                break;

            case MissionObjectiveType.ReachMetalAmount:
                progressData.progressLabel = GameTextProvider.UIText.MetalLabel;
                progressData.currentValue = Mathf.Max(0, gameData.metal);
                progressData.targetValue = Mathf.Max(1, mission.targetValue);
                break;

            case MissionObjectiveType.UnlockAllUpgradeCategories:
                progressData.progressLabel = GameTextProvider.UIText.UnlockedTabsProgressLabel;
                progressData.currentValue = CountUnlockedUpgradeCategories(upgradeManager);
                progressData.targetValue = 5;
                break;

            case MissionObjectiveType.ResearchEverythingPossible:
                progressData.progressLabel = GameTextProvider.UIText.MaxedResearchProgressLabel;
                progressData.currentValue = CountMaxedResearch(upgradeManager);
                progressData.targetValue = CountAllResearch(upgradeManager);
                break;

            default:
                progressData.progressLabel = GameTextProvider.UIText.ProgressLabel;
                progressData.currentValue = 0;
                progressData.targetValue = 1;
                break;
        }

        progressData.targetValue = Mathf.Max(1, progressData.targetValue);
        progressData.isCompleted = progressData.isCompleted || progressData.currentValue >= progressData.targetValue;
        return progressData;
    }

    private int CountUnlockedUpgradeCategories(UpgradeManager upgradeManager)
    {
        int unlockedCount = 0;

        if (upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.Miner))
        {
            unlockedCount++;
        }

        if (upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.Platform))
        {
            unlockedCount++;
        }

        if (upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.PowerStation))
        {
            unlockedCount++;
        }

        if (upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.Factory))
        {
            unlockedCount++;
        }

        if (upgradeManager.IsUpgradeCategoryUnlocked(UpgradeCategory.Shuttle))
        {
            unlockedCount++;
        }

        return unlockedCount;
    }

    private int CountAllResearch(UpgradeManager upgradeManager)
    {
        return upgradeManager.UpgradeStates.Count + upgradeManager.BuildingStates.Count;
    }

    private int CountMaxedResearch(UpgradeManager upgradeManager)
    {
        int completedCount = 0;

        for (int i = 0; i < upgradeManager.UpgradeStates.Count; i++)
        {
            if (upgradeManager.UpgradeStates[i].IsMaxLevel)
            {
                completedCount++;
            }
        }

        for (int i = 0; i < upgradeManager.BuildingStates.Count; i++)
        {
            if (upgradeManager.BuildingStates[i].IsMaxLevel)
            {
                completedCount++;
            }
        }

        return completedCount;
    }

    private string GetMetaBonusLevelKey(string bonusId)
    {
        return MetaBonusLevelKeyPrefix + bonusId;
    }
}
