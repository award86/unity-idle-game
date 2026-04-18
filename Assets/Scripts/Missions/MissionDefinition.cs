using System;
using UnityEngine;

[Serializable]
public class MissionDefinition
{
    public string id = "new_mission";
    public string missionName = "New Mission";
    public string missionNameRu = "";

    [TextArea]
    public string description = "Mission description";
    [TextArea]
    public string descriptionRu = "";

    public MissionObjectiveType objectiveType = MissionObjectiveType.ReachOreAmount;
    public int targetValue = 100;
    public int crystalReward = 1;
    public string DisplayName => GameTextProvider.GetText(missionName, missionNameRu);
    public string Description => GameTextProvider.GetText(description, descriptionRu);
}
