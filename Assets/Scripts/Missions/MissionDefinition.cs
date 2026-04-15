using System;
using UnityEngine;

[Serializable]
public class MissionDefinition
{
    public string id = "new_mission";
    public string missionName = "New Mission";

    [TextArea]
    public string description = "Mission description";

    public MissionObjectiveType objectiveType = MissionObjectiveType.ReachOreAmount;
    public int targetValue = 100;
    public int crystalReward = 1;
}
