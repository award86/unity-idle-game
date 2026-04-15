using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeConfig", menuName = "Idle Space/Upgrade Config")]
public class UpgradeConfig : ScriptableObject
{
    [SerializeField] private List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();

    public IReadOnlyList<UpgradeDefinition> Upgrades => upgrades;

    [ContextMenu("Fill With Example Upgrades")]
    private void FillWithExampleUpgrades()
    {
        upgrades = new List<UpgradeDefinition>
        {
            new UpgradeDefinition
            {
                id = "drill_gloves",
                upgradeName = "Drill Gloves",
                description = "Improves manual ore extraction on every tap.",
                category = UpgradeCategory.Miner,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 25)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.OrePerClick,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.7f,
                maxLevel = 5
            },
            new UpgradeDefinition
            {
                id = "mining_drones",
                upgradeName = "Mining Drones",
                description = "Adds passive ore extraction every second.",
                category = UpgradeCategory.Miner,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 20)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.OrePerSecond,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.6f,
                maxLevel = 10
            },
            new UpgradeDefinition
            {
                id = "battery_matrix",
                upgradeName = "Battery Matrix",
                description = "Expands your total energy capacity.",
                category = UpgradeCategory.PowerStation,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 30)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.EnergyCapacity,
                        valuePerLevel = 5f
                    }
                },
                costMultiplier = 1.65f,
                maxLevel = 8
            },
            new UpgradeDefinition
            {
                id = "solar_core",
                upgradeName = "Solar Core",
                description = "Improves energy regeneration output.",
                category = UpgradeCategory.PowerStation,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 35)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.EnergyRegenAmount,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.7f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "smelter_output",
                upgradeName = "Smelter Output",
                description = "Produces more metal from every smelting action.",
                category = UpgradeCategory.Factory,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 40)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.MetalProductionAmount,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.75f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "efficient_smelting",
                upgradeName = "Efficient Smelting",
                description = "Reduces ore needed for each metal craft.",
                category = UpgradeCategory.Factory,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 50),
                    new ResourceAmount(ResourceType.Metal, 5)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.MetalOreCostReduction,
                        valuePerLevel = 2f
                    }
                },
                costMultiplier = 1.8f,
                maxLevel = 8
            },
            new UpgradeDefinition
            {
                id = "shuttle_engine",
                upgradeName = "Shuttle Engine",
                description = "Boosts shuttle travel speed between base and mine.",
                category = UpgradeCategory.Shuttle,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Metal, 10)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.ShuttleTravelTimeReduction,
                        valuePerLevel = 5f
                    }
                },
                costMultiplier = 1.8f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "cargo_capacity",
                upgradeName = "Cargo Capacity",
                description = "Increases how much ore the shuttle can haul.",
                category = UpgradeCategory.Shuttle,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Metal, 8)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.ShuttleCapacity,
                        valuePerLevel = 25f
                    }
                },
                costMultiplier = 1.7f,
                maxLevel = 8
            },
            new UpgradeDefinition
            {
                id = "auto_shuttle_dispatch",
                upgradeName = "Auto Dispatch",
                description = "Launches the shuttle automatically when it is full.",
                category = UpgradeCategory.Shuttle,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Metal, 25)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.ShuttleAutoSend,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1f,
                maxLevel = 1
            }
        };
    }
}
