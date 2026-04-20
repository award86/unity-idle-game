using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeDefinition
{
    public string id = "new_upgrade";
    public string upgradeName = "New Upgrade";
    public string upgradeNameRu = "";
    public string upgradeNameEs = "";
    public string upgradeNameFr = "";
    public string upgradeNameDe = "";
    public string upgradeNameIt = "";
    public string upgradeNameZh = "";
    public string upgradeNameJa = "";
    public string upgradeNameAr = "";
    public string upgradeNameHe = "";

    [TextArea]
    public string description = "Upgrade description";
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

    public UpgradeCategory category = UpgradeCategory.Miner;
    public List<ResourceAmount> baseCosts = new List<ResourceAmount>();
    public List<UpgradeEffectDefinition> effects = new List<UpgradeEffectDefinition>();
    public float costMultiplier = 1.5f;
    public int maxLevel = 0;

    public IReadOnlyList<ResourceAmount> BaseCosts => baseCosts;
    public IReadOnlyList<UpgradeEffectDefinition> Effects => effects;
    public bool HasMaxLevel => maxLevel > 0;
    public UpgradeCategory ResolvedCategory => ResolveCategory();
    public string DisplayName => GameTextProvider.GetText(upgradeName, upgradeNameRu, upgradeNameEs, upgradeNameFr, upgradeNameDe, upgradeNameIt, upgradeNameZh, upgradeNameJa, upgradeNameAr, upgradeNameHe);
    public string Description => GameTextProvider.GetText(description, descriptionRu, descriptionEs, descriptionFr, descriptionDe, descriptionIt, descriptionZh, descriptionJa, descriptionAr, descriptionHe);

    private UpgradeCategory ResolveCategory()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            switch (effects[i].effectType)
            {
                case UpgradeEffectType.OrePerClick:
                case UpgradeEffectType.OrePerSecond:
                    return UpgradeCategory.Miner;

                case UpgradeEffectType.PlatformCapacity:
                case UpgradeEffectType.ShuttleLoadingTimeReduction:
                    return UpgradeCategory.Platform;

                case UpgradeEffectType.EnergyCapacity:
                case UpgradeEffectType.EnergyRegenAmount:
                case UpgradeEffectType.EnergyRegenIntervalReduction:
                    return UpgradeCategory.PowerStation;

                case UpgradeEffectType.MetalProductionAmount:
                case UpgradeEffectType.MetalOreCostReduction:
                case UpgradeEffectType.MetalEnergyCostReduction:
                    return UpgradeCategory.Factory;

                case UpgradeEffectType.ShuttleCapacity:
                case UpgradeEffectType.ShuttleTravelTimeReduction:
                case UpgradeEffectType.ShuttleAutoSend:
                case UpgradeEffectType.ShuttleCount:
                    return UpgradeCategory.Shuttle;
            }
        }

        return category;
    }
}
