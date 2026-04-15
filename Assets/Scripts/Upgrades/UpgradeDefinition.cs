using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeDefinition
{
    public string id = "new_upgrade";
    public string upgradeName = "New Upgrade";

    [TextArea]
    public string description = "Upgrade description";

    public UpgradeCategory category = UpgradeCategory.Miner;
    public List<ResourceAmount> baseCosts = new List<ResourceAmount>();
    public List<UpgradeEffectDefinition> effects = new List<UpgradeEffectDefinition>();
    public float costMultiplier = 1.5f;
    public int maxLevel = 0;

    public IReadOnlyList<ResourceAmount> BaseCosts => baseCosts;
    public IReadOnlyList<UpgradeEffectDefinition> Effects => effects;
    public bool HasMaxLevel => maxLevel > 0;
    public UpgradeCategory ResolvedCategory => ResolveCategory();

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
                    return UpgradeCategory.Shuttle;
            }
        }

        return category;
    }
}
