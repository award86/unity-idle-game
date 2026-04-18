using System.Collections.Generic;

public static class EffectTextFormatter
{
    public static string BuildEffectLine(UpgradeEffectType effectType, float effectValue)
    {
        string effectLabel = GameTextProvider.GetEffectLabel(effectType);

        switch (effectType)
        {
            case UpgradeEffectType.OrePerClick:
            case UpgradeEffectType.OrePerSecond:
            case UpgradeEffectType.EnergyCapacity:
            case UpgradeEffectType.EnergyRegenAmount:
            case UpgradeEffectType.MetalProductionAmount:
            case UpgradeEffectType.PlatformCapacity:
            case UpgradeEffectType.ShuttleCapacity:
            case UpgradeEffectType.ShuttleAutoSend:
            case UpgradeEffectType.ShuttleCount:
                return "+" + NumberFormatter.FormatFloat(effectValue) + " " + effectLabel;

            case UpgradeEffectType.EnergyRegenIntervalReduction:
            case UpgradeEffectType.MetalOreCostReduction:
            case UpgradeEffectType.MetalEnergyCostReduction:
            case UpgradeEffectType.ShuttleLoadingTimeReduction:
            case UpgradeEffectType.ShuttleTravelTimeReduction:
                return "-" + NumberFormatter.FormatFloat(effectValue) + "s " + effectLabel;

            default:
                return GameTextProvider.UIText.UnknownText;
        }
    }

    public static string BuildCostText(IReadOnlyList<ResourceAmount> costs)
    {
        if (costs == null || costs.Count <= 0)
        {
            return GameTextProvider.UIText.FreeText;
        }

        string[] costParts = new string[costs.Count];

        for (int i = 0; i < costs.Count; i++)
        {
            ResourceAmount cost = costs[i];
            costParts[i] = NumberFormatter.FormatInt(cost.amount) + " " + GetResourceLabel(cost.resourceType);
        }

        return string.Join(", ", costParts);
    }

    public static string GetResourceLabel(ResourceType resourceType)
    {
        return GameTextProvider.GetResourceLabel(resourceType);
    }
}
