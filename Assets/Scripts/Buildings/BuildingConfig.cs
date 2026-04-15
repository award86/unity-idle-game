using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingConfig", menuName = "Idle Space/Building Config")]
public class BuildingConfig : ScriptableObject
{
    [SerializeField] private List<BuildingDefinition> buildings = new List<BuildingDefinition>();

    public IReadOnlyList<BuildingDefinition> Buildings => buildings;

    [ContextMenu("Fill With Example Buildings")]
    private void FillWithExampleBuildings()
    {
        buildings = new List<BuildingDefinition>
        {
            new BuildingDefinition
            {
                id = "power_station",
                buildingName = "Power Station",
                description = "Expands total energy reserves and boosts regeneration.",
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 40)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.EnergyCapacity,
                        valuePerLevel = 5f
                    },
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.EnergyRegenAmount,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.7f,
                maxLevel = 10
            },
            new BuildingDefinition
            {
                id = "metal_factory",
                buildingName = "Metal Factory",
                description = "Improves metal output and reduces smelting waste.",
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 60)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.MetalProductionAmount,
                        valuePerLevel = 1f
                    },
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.MetalOreCostReduction,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.8f,
                maxLevel = 8
            },
            new BuildingDefinition
            {
                id = "mining_platform",
                buildingName = "Mining Platform",
                description = "Expands platform storage and supports ore extraction on site.",
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 50)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.PlatformCapacity,
                        valuePerLevel = 50f
                    },
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.OrePerSecond,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.75f,
                maxLevel = 10
            }
        };
    }
}
