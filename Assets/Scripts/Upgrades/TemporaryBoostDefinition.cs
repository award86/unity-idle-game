using System;
using UnityEngine;

[Serializable]
public class TemporaryBoostDefinition
{
    public string id = "new_temporary_boost";
    public string boostName = "New Temporary Boost";
    public string boostNameRu = "";

    [TextArea]
    public string description = "Temporary boost description";
    [TextArea]
    public string descriptionRu = "";

    public float multiplier = 2f;
    public TemporaryBoostTargetType targetType = TemporaryBoostTargetType.OrePerSecond;
    public float durationSeconds = 90f;
    public TemporaryBoostAvailabilityType availabilityType = TemporaryBoostAvailabilityType.ByTime;
    public float appearanceIntervalSeconds = 120f;
    public int oreRequiredForAppearance = 500;
    public string DisplayName => GameTextProvider.GetText(boostName, boostNameRu);
    public string Description => GameTextProvider.GetText(description, descriptionRu);
}
