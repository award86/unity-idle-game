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
                id = "mining_platform",
                buildingName = "Mining Platform",
                description = "Expands platform storage and adds passive ore extraction on site.",
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 30)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.PlatformCapacity,
                        valuePerLevel = 25f
                    },
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.OrePerSecond,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.65f,
                maxLevel = 8
            },
            new BuildingDefinition
            {
                id = "power_station",
                buildingName = "Power Station",
                description = "Activates the energy grid and improves battery output.",
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 55)
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
                costMultiplier = 1.75f,
                maxLevel = 8
            },
            new BuildingDefinition
            {
                id = "metal_factory",
                buildingName = "Metal Factory",
                description = "Unlocks metal production and improves each refining batch.",
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 90),
                    new ResourceAmount(ResourceType.Energy, 10)
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
                maxLevel = 6
            }
        };
    }
}
