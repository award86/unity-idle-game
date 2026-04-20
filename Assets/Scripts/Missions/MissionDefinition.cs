using System;
using UnityEngine;

[Serializable]
public class MissionDefinition
{
    public string id = "new_mission";
    public string missionName = "New Mission";
    public string missionNameRu = "";
    public string missionNameEs = "";
    public string missionNameFr = "";
    public string missionNameDe = "";
    public string missionNameIt = "";
    public string missionNameZh = "";
    public string missionNameJa = "";
    public string missionNameAr = "";
    public string missionNameHe = "";

    [TextArea]
    public string description = "Mission description";
    [TextArea]
    public string descriptionRu = "";
    [TextArea]
    public string descriptionEs = "";
    [TextArea]
    public string descriptionFr = "";
    [TextArea]
    public string descriptionDe = "";
    [TextArea]
    public string descriptionIt = "";
    [TextArea]
    public string descriptionZh = "";
    [TextArea]
    public string descriptionJa = "";
    [TextArea]
    public string descriptionAr = "";
    [TextArea]
    public string descriptionHe = "";

    public MissionObjectiveType objectiveType = MissionObjectiveType.ReachOreAmount;
    public int targetValue = 100;
    public int crystalReward = 1;
    public string DisplayName => GameTextProvider.GetText(missionName, missionNameRu, missionNameEs, missionNameFr, missionNameDe, missionNameIt, missionNameZh, missionNameJa, missionNameAr, missionNameHe);
    public string Description => GameTextProvider.GetText(description, descriptionRu, descriptionEs, descriptionFr, descriptionDe, descriptionIt, descriptionZh, descriptionJa, descriptionAr, descriptionHe);
}
