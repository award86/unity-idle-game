using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TemporaryBoostConfig", menuName = "Idle Space/Temporary Boost Config")]
public class TemporaryBoostConfig : ScriptableObject
{
    [SerializeField] private List<TemporaryBoostDefinition> temporaryBoosts = new List<TemporaryBoostDefinition>();

    public IReadOnlyList<TemporaryBoostDefinition> TemporaryBoosts => temporaryBoosts;

    [ContextMenu("Fill With Example Temporary Boosts")]
    private void FillWithExampleTemporaryBoosts()
    {
        temporaryBoosts = new List<TemporaryBoostDefinition>
        {
            new TemporaryBoostDefinition
            {
                id = "quantum_rush",
                boostName = "Quantum Rush",
                description = "Temporary x2 Ore / sec boost that appears after enough ore is earned.",
                multiplier = 2f,
                targetType = TemporaryBoostTargetType.OrePerSecond,
                durationSeconds = 90f,
                availabilityType = TemporaryBoostAvailabilityType.ByAccumulatedOre,
                oreRequiredForAppearance = 250,
                appearanceIntervalSeconds = 120f
            },
            new TemporaryBoostDefinition
            {
                id = "plasma_hands",
                boostName = "Plasma Hands",
                description = "Temporary x3 Ore / click boost that appears over time.",
                multiplier = 3f,
                targetType = TemporaryBoostTargetType.OrePerClick,
                durationSeconds = 60f,
                availabilityType = TemporaryBoostAvailabilityType.ByTime,
                oreRequiredForAppearance = 0,
                appearanceIntervalSeconds = 180f
            },
            new TemporaryBoostDefinition
            {
                id = "cargo_overclock",
                boostName = "Cargo Overclock",
                description = "Temporary x1.5 Ore / sec boost that appears after more ore is earned.",
                multiplier = 1.5f,
                targetType = TemporaryBoostTargetType.OrePerSecond,
                durationSeconds = 120f,
                availabilityType = TemporaryBoostAvailabilityType.ByAccumulatedOre,
                oreRequiredForAppearance = 600,
                appearanceIntervalSeconds = 120f
            }
        };
    }
}
