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
                boostNameRu = "Квантовый рывок",
                description = "Temporary x2 Ore / sec boost that appears after enough ore is earned.",
                descriptionRu = "Временный x2 к руде в секунду, появляется после накопления руды.",
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
                boostNameRu = "Плазменные руки",
                description = "Temporary x3 Ore / click boost that appears over time.",
                descriptionRu = "Временный x3 к руде за клик, появляется со временем.",
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
                boostNameRu = "Грузовой оверклок",
                description = "Temporary x1.5 Ore / sec boost that appears after more ore is earned.",
                descriptionRu = "Временный x1.5 к руде в секунду, появляется после добычи большего количества руды.",
                multiplier = 1.5f,
                targetType = TemporaryBoostTargetType.OrePerSecond,
                durationSeconds = 120f,
                availabilityType = TemporaryBoostAvailabilityType.ByAccumulatedOre,
                oreRequiredForAppearance = 600,
                appearanceIntervalSeconds = 120f
            },
            new TemporaryBoostDefinition
            {
                id = "warp_corridor",
                boostName = "Warp Corridor",
                boostNameRu = "Варп-коридор",
                description = "Cuts shuttle travel time in half for a short window.",
                descriptionRu = "На короткое время сокращает длительность полёта шаттла вдвое.",
                multiplier = 2f,
                targetType = TemporaryBoostTargetType.ShuttleTravelSpeed,
                durationSeconds = 300f,
                availabilityType = TemporaryBoostAvailabilityType.ByTime,
                oreRequiredForAppearance = 0,
                appearanceIntervalSeconds = 240f
            }
        };
    }
}
