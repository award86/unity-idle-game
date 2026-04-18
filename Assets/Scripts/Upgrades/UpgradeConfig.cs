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
                upgradeNameRu = "Перчатки бурильщика",
                description = "Improves manual ore extraction on every tap.",
                descriptionRu = "Улучшает ручную добычу руды при каждом нажатии.",
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
                upgradeNameRu = "Дроны-добытчики",
                description = "Adds passive ore extraction every second.",
                descriptionRu = "Добавляет пассивную добычу руды каждую секунду.",
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
                upgradeNameRu = "Складские ящики",
                description = "Adds extra ore storage to the mining platform.",
                descriptionRu = "Добавляет дополнительное место для хранения руды на платформе.",
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
                upgradeNameRu = "Рудные силосы",
                description = "Massively expands long-term storage on the platform.",
                descriptionRu = "Сильно увеличивает долгосрочное хранение руды на платформе.",
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
                upgradeNameRu = "Погрузочные рельсы",
                description = "Speeds up ore transfer from the platform into the shuttle.",
                descriptionRu = "Ускоряет перегрузку руды с платформы в шаттл.",
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
                upgradeNameRu = "Батарейная матрица",
                description = "Expands your total energy reserves.",
                descriptionRu = "Увеличивает общий запас энергии.",
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
                upgradeNameRu = "Солнечное ядро",
                description = "Improves energy regeneration output.",
                descriptionRu = "Улучшает восстановление энергии.",
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
                upgradeNameRu = "Контур охлаждения",
                description = "Reduces the delay between energy regeneration ticks.",
                descriptionRu = "Сокращает задержку между тиками восстановления энергии.",
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
                upgradeNameRu = "Выход плавильни",
                description = "Produces more metal from every refining action.",
                descriptionRu = "Даёт больше металла за каждое действие переработки.",
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
                upgradeNameRu = "Эффективная плавка",
                description = "Reduces ore needed for each metal craft.",
                descriptionRu = "Снижает расход руды на каждый крафт металла.",
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
                upgradeNameRu = "Энергомаршрутизация",
                description = "Cuts the energy required for each metal craft.",
                descriptionRu = "Снижает затраты энергии на каждый крафт металла.",
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
                id = "fleet_hangar",
                upgradeName = "Fleet Hangar",
                upgradeNameRu = "Ангар флота",
                description = "Deploys additional ore shuttles to the platform fleet.",
                descriptionRu = "Добавляет дополнительные шаттлы к платформенному флоту.",
                category = UpgradeCategory.Shuttle,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Metal, 14),
                    new ResourceAmount(ResourceType.Ore, 90)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.ShuttleCount,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 2f,
                maxLevel = 2
            },
            new UpgradeDefinition
            {
                id = "shuttle_engine",
                upgradeName = "Shuttle Engine",
                upgradeNameRu = "Двигатель шаттла",
                description = "Cuts travel time between the platform and the warehouse.",
                descriptionRu = "Сокращает время полёта между платформой и складом.",
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
                upgradeNameRu = "Грузовая вместимость",
                description = "Increases how much ore the shuttle can haul per trip.",
                descriptionRu = "Увеличивает, сколько руды шаттл может перевозить за рейс.",
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
                upgradeNameRu = "Автоотправка",
                description = "Unlocks auto dispatch for one more shuttle in your fleet.",
                descriptionRu = "Открывает автоотправку ещё для одного шаттла во флоте.",
                category = UpgradeCategory.Shuttle,
                baseCosts = new List<ResourceAmount>
                {
                    new ResourceAmount(ResourceType.Metal, 20),
                    new ResourceAmount(ResourceType.Ore, 60)
                },
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.ShuttleAutoSend,
                        valuePerLevel = 1f
                    }
                },
                costMultiplier = 1.9f,
                maxLevel = 3
            }
        };
    }
}
