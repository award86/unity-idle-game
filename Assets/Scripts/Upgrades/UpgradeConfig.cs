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
                    new ResourceAmount(ResourceType.Ore, 10)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.OrePerClick,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.6f,
                maxLevel = 8
            },
            new UpgradeDefinition
            {
                id = "mining_drones",
                upgradeName = "Mining Drones",
                description = "Adds passive ore extraction every second.",
                category = UpgradeCategory.Miner,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 24)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.OrePerSecond,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.75f,
                maxLevel = 10
            },
            new UpgradeDefinition
            {
                id = "storage_crates",
                upgradeName = "Storage Crates",
                description = "Adds extra ore storage to the mining platform.",
                category = UpgradeCategory.Platform,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 35)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.PlatformCapacity,
                        valuePerLevel = 25f
                    }
                },
                costMultiplier = 1.65f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "ore_silos",
                upgradeName = "Ore Silos",
                description = "Massively expands long-term storage on the platform.",
                category = UpgradeCategory.Platform,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 70),
                    new ResourceAmount(ResourceType.Metal, 4)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.PlatformCapacity,
                        valuePerLevel = 50f
                    }
                },
                costMultiplier = 1.8f,
                maxLevel = 5
            },
            new UpgradeDefinition
            {
                id = "loader_rails",
                upgradeName = "Loader Rails",
                description = "Speeds up ore transfer from the platform into the shuttle.",
                category = UpgradeCategory.Platform,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 55),
                    new ResourceAmount(ResourceType.Metal, 3)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.ShuttleLoadingTimeReduction,
                        valuePerLevel = 0.25f
                    }
                },
                costMultiplier = 1.75f,
                maxLevel = 5
            },
            new UpgradeDefinition
            {
                id = "battery_matrix",
                upgradeName = "Battery Matrix",
                description = "Expands your total energy reserves.",
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
                        valuePerLevel = 10f
                    }
                },
                costMultiplier = 1.65f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "solar_core",
                upgradeName = "Solar Core",
                description = "Improves energy regeneration output.",
                category = UpgradeCategory.PowerStation,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 45),
                    new ResourceAmount(ResourceType.Metal, 3)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.EnergyRegenAmount,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.75f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "coolant_loop",
                upgradeName = "Coolant Loop",
                description = "Reduces the delay between energy regeneration ticks.",
                category = UpgradeCategory.PowerStation,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 65),
                    new ResourceAmount(ResourceType.Metal, 6)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.EnergyRegenIntervalReduction,
                        valuePerLevel = 0.35f
                    }
                },
                costMultiplier = 1.85f,
                maxLevel = 5
            },
            new UpgradeDefinition
            {
                id = "smelter_output",
                upgradeName = "Smelter Output",
                description = "Produces more metal from every refining action.",
                category = UpgradeCategory.Factory,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 50),
                    new ResourceAmount(ResourceType.Metal, 4)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.MetalProductionAmount,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.8f,
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
                    new ResourceAmount(ResourceType.Ore, 60),
                    new ResourceAmount(ResourceType.Metal, 6)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.MetalOreCostReduction,
                        valuePerLevel = 2f
                    }
                },
                costMultiplier = 1.85f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "power_routing",
                upgradeName = "Power Routing",
                description = "Cuts the energy required for each metal craft.",
                category = UpgradeCategory.Factory,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 75),
                    new ResourceAmount(ResourceType.Metal, 8)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.MetalEnergyCostReduction,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.9f,
                maxLevel = 4
            },
            new UpgradeDefinition
            {
                id = "shuttle_engine",
                upgradeName = "Shuttle Engine",
                description = "Cuts travel time between the platform and the warehouse.",
                category = UpgradeCategory.Shuttle,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Metal, 6)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.ShuttleTravelTimeReduction,
                        valuePerLevel = 1.5f
                    }
                },
                costMultiplier = 1.7f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "cargo_capacity",
                upgradeName = "Cargo Capacity",
                description = "Increases how much ore the shuttle can haul per trip.",
                category = UpgradeCategory.Shuttle,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Ore, 25),
                    new ResourceAmount(ResourceType.Metal, 8)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.ShuttleCapacity,
                        valuePerLevel = 10f
                    }
                },
                costMultiplier = 1.75f,
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
                    new ResourceAmount(ResourceType.Metal, 20)
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
