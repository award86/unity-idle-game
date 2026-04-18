using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MetaBonusConfig", menuName = "Idle Space/Meta Bonus Config")]
public class MetaBonusConfig : ScriptableObject
{
    [SerializeField] private List<MetaBonusDefinition> bonuses = new List<MetaBonusDefinition>();

    public IReadOnlyList<MetaBonusDefinition> Bonuses => bonuses;

    public static MetaBonusConfig CreateRuntimeDefault()
    {
        MetaBonusConfig config = CreateInstance<MetaBonusConfig>();
        config.PopulateExampleBonuses();
        return config;
    }

    [ContextMenu("Fill With Example Meta Bonuses")]
    private void FillWithExampleMetaBonuses()
    {
        PopulateExampleBonuses();
    }

    private void PopulateExampleBonuses()
    {
        bonuses = new List<MetaBonusDefinition>
        {
            new MetaBonusDefinition
            {
                id = "legacy_drills",
                bonusName = "Legacy Drills",
                bonusNameRu = "Наследные буры",
                description = "Permanent bonus to manual ore mining in all future missions.",
                descriptionRu = "Постоянный бонус к ручной добыче руды во всех следующих миссиях.",
                crystalCost = 3,
                costMultiplier = 1.8f,
                maxLevel = 5,
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.OrePerClick,
                        valuePerLevel = 1f
                    }
                }
            },
            new MetaBonusDefinition
            {
                id = "relay_archive",
                bonusName = "Relay Archive",
                bonusNameRu = "Архив ретранслятора",
                description = "Permanent bonus to passive ore production.",
                descriptionRu = "Постоянный бонус к пассивной добыче руды.",
                crystalCost = 4,
                costMultiplier = 1.85f,
                maxLevel = 5,
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.OrePerSecond,
                        valuePerLevel = 1f
                    }
                }
            },
            new MetaBonusDefinition
            {
                id = "battery_blueprints",
                bonusName = "Battery Blueprints",
                bonusNameRu = "Чертежи батарей",
                description = "Start each mission with more total energy capacity.",
                descriptionRu = "Начинайте каждую миссию с большим общим запасом энергии.",
                crystalCost = 5,
                costMultiplier = 1.9f,
                maxLevel = 4,
                effects = new List<UpgradeEffectDefinition>
                {
                    new UpgradeEffectDefinition
                    {
                        effectType = UpgradeEffectType.EnergyCapacity,
                        valuePerLevel = 5f
                    }
                }
            }
        };
    }
}
