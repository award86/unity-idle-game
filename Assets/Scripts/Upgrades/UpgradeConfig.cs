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
                id = "spacesuit_efficiency",
                upgradeName = "Spacesuit Mk I",
                description = "Reinforced suit boosts manual mining efficiency.",
                category = UpgradeCategory.Mining,
                effectType = UpgradeEffectType.OrePerClickMultiplier,
                baseCost = 25,
                costMultiplier = 1.7f,
                baseValue = 0.25f,
                valuePerLevel = 0.15f,
                maxLevel = 5
            },
            new UpgradeDefinition
            {
                id = "plasma_tool",
                upgradeName = "Plasma Tool",
                description = "Sharper tools extract more ore per manual click.",
                category = UpgradeCategory.Mining,
                effectType = UpgradeEffectType.OrePerClickFlat,
                baseCost = 15,
                costMultiplier = 1.55f,
                baseValue = 1f,
                valuePerLevel = 1f,
                maxLevel = 10
            },
            new UpgradeDefinition
            {
                id = "astronaut_training",
                upgradeName = "Astronaut Training",
                description = "Improves the crew's passive mining output.",
                category = UpgradeCategory.Mining,
                effectType = UpgradeEffectType.OrePerSecondFlat,
                baseCost = 20,
                costMultiplier = 1.6f,
                baseValue = 1f,
                valuePerLevel = 1f,
                maxLevel = 12
            },
            new UpgradeDefinition
            {
                id = "shuttle_engine",
                upgradeName = "Shuttle Engine",
                description = "Faster engines improve delivery speed.",
                category = UpgradeCategory.Shuttle,
                effectType = UpgradeEffectType.OrePerSecondMultiplier,
                baseCost = 40,
                costMultiplier = 1.8f,
                baseValue = 0.20f,
                valuePerLevel = 0.10f,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "cargo_capacity",
                upgradeName = "Cargo Capacity",
                description = "Bigger cargo pods increase shuttle output.",
                category = UpgradeCategory.Shuttle,
                effectType = UpgradeEffectType.OrePerSecondFlat,
                baseCost = 35,
                costMultiplier = 1.7f,
                baseValue = 2f,
                valuePerLevel = 2f,
                maxLevel = 8
            },
            new UpgradeDefinition
            {
                id = "quantum_rush",
                upgradeName = "Quantum Rush",
                description = "Temporary x2 income boost for 90 seconds.",
                category = UpgradeCategory.TemporaryBoost,
                effectType = UpgradeEffectType.TemporaryIncomeMultiplier,
                baseCost = 120,
                costMultiplier = 1.35f,
                baseValue = 2f,
                valuePerLevel = 0f,
                durationSeconds = 90f
            }
        };
    }
}
