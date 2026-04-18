using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionConfig", menuName = "Idle Space/Mission Config")]
public class MissionConfig : ScriptableObject
{
    [SerializeField] private List<MissionDefinition> missions = new List<MissionDefinition>();

    public IReadOnlyList<MissionDefinition> Missions => missions;

    public static MissionConfig CreateRuntimeDefault()
    {
        MissionConfig config = CreateInstance<MissionConfig>();
        config.PopulateExampleMissions();
        return config;
    }

    [ContextMenu("Fill With Example Missions")]
    private void FillWithExampleMissions()
    {
        PopulateExampleMissions();
    }

    private void PopulateExampleMissions()
    {
        missions = new List<MissionDefinition>
        {
            new MissionDefinition
            {
                id = "ore_stockpile",
                missionName = "Ore Stockpile",
                missionNameRu = "Запас руды",
                description = "Accumulate a solid first warehouse reserve.",
                descriptionRu = "Накопите первый серьёзный запас руды на складе.",
                objectiveType = MissionObjectiveType.ReachOreAmount,
                targetValue = 150,
                crystalReward = 3
            },
            new MissionDefinition
            {
                id = "metal_chain",
                missionName = "Metal Chain",
                missionNameRu = "Металлическая цепочка",
                description = "Establish metal processing and collect enough refined metal.",
                descriptionRu = "Наладьте переработку металла и соберите достаточно очищенного металла.",
                objectiveType = MissionObjectiveType.ReachMetalAmount,
                targetValue = 20,
                crystalReward = 4
            },
            new MissionDefinition
            {
                id = "open_all_tabs",
                missionName = "Full Research Grid",
                missionNameRu = "Полная сетка исследований",
                description = "Unlock every upgrade branch in the colony.",
                descriptionRu = "Откройте каждую ветку улучшений в колонии.",
                objectiveType = MissionObjectiveType.UnlockAllUpgradeCategories,
                targetValue = 0,
                crystalReward = 5
            },
            new MissionDefinition
            {
                id = "max_everything",
                missionName = "Research Complete",
                missionNameRu = "Исследования завершены",
                description = "Finish every building and every permanent upgrade.",
                descriptionRu = "Завершите все постройки и все постоянные улучшения.",
                objectiveType = MissionObjectiveType.ResearchEverythingPossible,
                targetValue = 0,
                crystalReward = 8
            }
        };
    }
}
