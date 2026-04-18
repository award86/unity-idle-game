public static class GameTextProvider
{
    private static readonly GameUiTextConfig DefaultUiText = new GameUiTextConfig();
    private static ShuttleConfig configuredGameConfig;

    public static GameUiTextConfig UIText =>
        configuredGameConfig != null && configuredGameConfig.UIText != null
            ? configuredGameConfig.UIText
            : DefaultUiText;

    public static string NoMissionsText =>
        configuredGameConfig != null
            ? configuredGameConfig.NoMissionsText
            : ShuttleConfig.DefaultNoMissionsText;

    public static void Configure(ShuttleConfig gameConfig)
    {
        configuredGameConfig = gameConfig;
    }

    public static string GetResourceLabel(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Ore:
                return UIText.OreLabel;

            case ResourceType.Energy:
                return UIText.EnergyLabel;

            case ResourceType.Metal:
                return UIText.MetalLabel;

            case ResourceType.Crystal:
                return UIText.CrystalLabel;

            default:
                return resourceType.ToString();
        }
    }

    public static string GetEffectLabel(UpgradeEffectType effectType)
    {
        switch (effectType)
        {
            case UpgradeEffectType.OrePerClick:
                return UIText.OrePerClickEffectText;

            case UpgradeEffectType.OrePerSecond:
                return UIText.OrePerSecondEffectText;

            case UpgradeEffectType.EnergyCapacity:
                return UIText.EnergyCapacityEffectText;

            case UpgradeEffectType.EnergyRegenAmount:
                return UIText.EnergyRegenEffectText;

            case UpgradeEffectType.EnergyRegenIntervalReduction:
                return UIText.EnergyIntervalEffectText;

            case UpgradeEffectType.MetalProductionAmount:
                return UIText.MetalPerCraftEffectText;

            case UpgradeEffectType.MetalOreCostReduction:
                return UIText.OreCraftCostEffectText;

            case UpgradeEffectType.MetalEnergyCostReduction:
                return UIText.EnergyCraftCostEffectText;

            case UpgradeEffectType.PlatformCapacity:
                return UIText.PlatformCapacityEffectText;

            case UpgradeEffectType.ShuttleCapacity:
                return UIText.ShuttleCapacityEffectText;

            case UpgradeEffectType.ShuttleLoadingTimeReduction:
                return UIText.ShuttleLoadingEffectText;

            case UpgradeEffectType.ShuttleTravelTimeReduction:
                return UIText.ShuttleTravelEffectText;

            case UpgradeEffectType.ShuttleAutoSend:
                return UIText.AutoDispatchShuttleEffectText;

            case UpgradeEffectType.ShuttleCount:
                return UIText.ShuttleCountEffectText;

            default:
                return UIText.UnknownText;
        }
    }
}
