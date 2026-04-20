using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingDefinition
{
    public string id = "new_building";
    public string buildingName = "New Building";
    public string buildingNameRu = "";
    public string buildingNameEs = "";
    public string buildingNameFr = "";
    public string buildingNameDe = "";
    public string buildingNameIt = "";
    public string buildingNameZh = "";
    public string buildingNameJa = "";
    public string buildingNameAr = "";
    public string buildingNameHe = "";

    [TextArea]
    public string description = "Building description";
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

    public List<ResourceAmount> baseCosts = new List<ResourceAmount>();
    public List<UpgradeEffectDefinition> effects = new List<UpgradeEffectDefinition>();
    public float costMultiplier = 1.5f;
    public int maxLevel = 0;

    public IReadOnlyList<ResourceAmount> BaseCosts => baseCosts;
    public IReadOnlyList<UpgradeEffectDefinition> Effects => effects;
    public bool HasMaxLevel => maxLevel > 0;
    public string DisplayName => GameTextProvider.GetText(buildingName, buildingNameRu, buildingNameEs, buildingNameFr, buildingNameDe, buildingNameIt, buildingNameZh, buildingNameJa, buildingNameAr, buildingNameHe);
    public string Description => GameTextProvider.GetText(description, descriptionRu, descriptionEs, descriptionFr, descriptionDe, descriptionIt, descriptionZh, descriptionJa, descriptionAr, descriptionHe);
}
