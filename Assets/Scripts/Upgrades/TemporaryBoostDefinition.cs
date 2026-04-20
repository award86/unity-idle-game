using System;
using UnityEngine;

[Serializable]
public class TemporaryBoostDefinition
{
    public string id = "new_temporary_boost";
    public string boostName = "New Temporary Boost";
    public string boostNameRu = "";
    public string boostNameEs = "";
    public string boostNameFr = "";
    public string boostNameDe = "";
    public string boostNameIt = "";
    public string boostNameZh = "";
    public string boostNameJa = "";
    public string boostNameAr = "";
    public string boostNameHe = "";

    [TextArea]
    public string description = "Temporary boost description";
    [TextArea]
    public string descriptionRu = "";
    [TextArea]
    public string descriptionEs = "";
    [TextArea]
    public string descriptionFr = "";
    [TextArea]
    public string descriptionDe = "";
    [TextArea]
    public string descriptionIt = "";
    [TextArea]
    public string descriptionZh = "";
    [TextArea]
    public string descriptionJa = "";
    [TextArea]
    public string descriptionAr = "";
    [TextArea]
    public string descriptionHe = "";

    public float multiplier = 2f;
    public TemporaryBoostTargetType targetType = TemporaryBoostTargetType.OrePerSecond;
    public float durationSeconds = 90f;
    public TemporaryBoostAvailabilityType availabilityType = TemporaryBoostAvailabilityType.ByTime;
    public float appearanceIntervalSeconds = 120f;
    public int oreRequiredForAppearance = 500;
    public string DisplayName => GameTextProvider.GetText(boostName, boostNameRu, boostNameEs, boostNameFr, boostNameDe, boostNameIt, boostNameZh, boostNameJa, boostNameAr, boostNameHe);
    public string Description => GameTextProvider.GetText(description, descriptionRu, descriptionEs, descriptionFr, descriptionDe, descriptionIt, descriptionZh, descriptionJa, descriptionAr, descriptionHe);
}
