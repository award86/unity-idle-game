using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingDefinition
{
    public string id = "new_building";
    public string buildingName = "New Building";

    [TextArea]
    public string description = "Building description";

    public List<ResourceAmount> baseCosts = new List<ResourceAmount>();
    public List<UpgradeEffectDefinition> effects = new List<UpgradeEffectDefinition>();
    public float costMultiplier = 1.5f;
    public int maxLevel = 0;

    public IReadOnlyList<ResourceAmount> BaseCosts => baseCosts;
    public IReadOnlyList<UpgradeEffectDefinition> Effects => effects;
    public bool HasMaxLevel => maxLevel > 0;
}
