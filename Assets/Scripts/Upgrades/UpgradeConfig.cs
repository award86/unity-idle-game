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
                effectType = UpgradeEffectType.MiningPerClick,
                baseCost = 25,
                costMultiplier = 1.7f,
                valuePerLevel = 1f,
                maxLevel = 5
            },
            new UpgradeDefinition
            {
                id = "plasma_tool",
                upgradeName = "Plasma Tool",
                description = "Sharper tools extract more ore per manual click.",
                effectType = UpgradeEffectType.MiningPerClick,
                baseCost = 15,
                costMultiplier = 1.55f,
                valuePerLevel = 2f,
                maxLevel = 10
            },
            new UpgradeDefinition
            {
                id = "astronaut_training",
                upgradeName = "Astronaut Training",
                description = "Improves the crew's passive mining output.",
                effectType = UpgradeEffectType.MiningPerSecond,
                baseCost = 20,
                costMultiplier = 1.6f,
                valuePerLevel = 1f,
                maxLevel = 12
            },
            new UpgradeDefinition
            {
                id = "shuttle_engine",
                upgradeName = "Shuttle Engine",
                description = "Faster engines improve delivery speed.",
                effectType = UpgradeEffectType.Shuttle,
                baseCost = 40,
                costMultiplier = 1.8f,
                valuePerLevel = 0f,
                shuttleTravelTimeReductionPerLevel = 5f,
                shuttleCapacityIncreasePerLevel = 0,
                maxLevel = 6
            },
            new UpgradeDefinition
            {
                id = "cargo_capacity",
                upgradeName = "Cargo Capacity",
                description = "Bigger cargo pods increase shuttle output.",
                effectType = UpgradeEffectType.Shuttle,
                baseCost = 35,
                costMultiplier = 1.7f,
                valuePerLevel = 0f,
                shuttleTravelTimeReductionPerLevel = 0f,
                shuttleCapacityIncreasePerLevel = 25,
                maxLevel = 8
            },
            new UpgradeDefinition
            {
                id = "auto_shuttle_dispatch",
                upgradeName = "Auto Dispatch",
                description = "Launches the shuttle automatically as soon as it is full.",
                effectType = UpgradeEffectType.ShuttleAutoSend,
                baseCost = 120,
                costMultiplier = 1f,
                valuePerLevel = 0f,
                shuttleTravelTimeReductionPerLevel = 0f,
                shuttleCapacityIncreasePerLevel = 0,
                maxLevel = 1
            }
        };
    }
}
