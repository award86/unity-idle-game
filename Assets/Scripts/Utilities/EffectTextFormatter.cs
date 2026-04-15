using System.Collections.Generic;

public static class EffectTextFormatter
{
    public static string BuildEffectLine(UpgradeEffectType effectType, float effectValue)
    {
        switch (effectType)
        {
            case UpgradeEffectType.OrePerClick:
                return "+" + NumberFormatter.FormatFloat(effectValue) + " Ore / click";

            case UpgradeEffectType.OrePerSecond:
                return "+" + NumberFormatter.FormatFloat(effectValue) + " Ore / sec";

            case UpgradeEffectType.EnergyCapacity:
                return "+" + NumberFormatter.FormatFloat(effectValue) + " Energy cap";

            case UpgradeEffectType.EnergyRegenAmount:
                return "+" + NumberFormatter.FormatFloat(effectValue) + " Energy regen";

            case UpgradeEffectType.EnergyRegenIntervalReduction:
                return "-" + NumberFormatter.FormatFloat(effectValue) + "s Energy interval";

            case UpgradeEffectType.MetalProductionAmount:
                return "+" + NumberFormatter.FormatFloat(effectValue) + " Metal / craft";

            case UpgradeEffectType.MetalOreCostReduction:
                return "-" + NumberFormatter.FormatFloat(effectValue) + " Ore craft cost";

            case UpgradeEffectType.MetalEnergyCostReduction:
                return "-" + NumberFormatter.FormatFloat(effectValue) + " Energy craft cost";

            case UpgradeEffectType.PlatformCapacity:
                return "+" + NumberFormatter.FormatFloat(effectValue) + " Platform capacity";

            case UpgradeEffectType.ShuttleCapacity:
                return "+" + NumberFormatter.FormatFloat(effectValue) + " Shuttle capacity";

            case UpgradeEffectType.ShuttleLoadingTimeReduction:
                return "-" + NumberFormatter.FormatFloat(effectValue) + "s Shuttle loading";

            case UpgradeEffectType.ShuttleTravelTimeReduction:
                return "-" + NumberFormatter.FormatFloat(effectValue) + "s Shuttle travel";

            case UpgradeEffectType.ShuttleAutoSend:
                return "Unlock auto dispatch";

            default:
                return "Unknown";
        }
    }

    public static string BuildCostText(IReadOnlyList<ResourceAmount> costs)
    {
        if (costs == null || costs.Count <= 0)
        {
            return "Free";
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
        switch (resourceType)
        {
            case ResourceType.Ore:
                return "Ore";

            case ResourceType.Energy:
                return "Energy";

            case ResourceType.Metal:
                return "Metal";

            case ResourceType.Crystal:
                return "Crystal";

            default:
                return resourceType.ToString();
        }
    }
}
