public static class GameTextProvider
{
    private static readonly GameLanguage[] availableLanguages =
    {
        GameLanguage.English,
        GameLanguage.Russian,
        GameLanguage.Spanish,
        GameLanguage.French,
        GameLanguage.German,
        GameLanguage.Italian,
        GameLanguage.Chinese,
        GameLanguage.Japanese,
        GameLanguage.Arabic,
        GameLanguage.Hebrew
    };

    private static readonly GameUiTextConfig DefaultUiText = new GameUiTextConfig();
    private static readonly GameUiTextConfig DefaultRussianUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Russian);
    private static readonly GameUiTextConfig DefaultSpanishUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Spanish);
    private static readonly GameUiTextConfig DefaultFrenchUiText = GameUiTextConfig.CreateDefaults(GameLanguage.French);
    private static readonly GameUiTextConfig DefaultGermanUiText = GameUiTextConfig.CreateDefaults(GameLanguage.German);
    private static readonly GameUiTextConfig DefaultItalianUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Italian);
    private static readonly GameUiTextConfig DefaultChineseUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Chinese);
    private static readonly GameUiTextConfig DefaultJapaneseUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Japanese);
    private static readonly GameUiTextConfig DefaultArabicUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Arabic);
    private static readonly GameUiTextConfig DefaultHebrewUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Hebrew);
    private static ShuttleConfig configuredGameConfig;
    private static GameLanguage currentLanguage = GameLanguage.English;

    public static GameLanguage[] AvailableLanguages => availableLanguages;
    public static GameLanguage CurrentLanguage => currentLanguage;

    public static GameUiTextConfig UIText => configuredGameConfig != null
        ? configuredGameConfig.GetUIText(CurrentLanguage)
        : GetDefaultUIText(CurrentLanguage);

    public static string NoMissionsText => configuredGameConfig != null
        ? configuredGameConfig.GetNoMissionsText(CurrentLanguage)
        : GetDefaultNoMissionsText(CurrentLanguage);

    public static void Configure(ShuttleConfig gameConfig)
    {
        configuredGameConfig = gameConfig;
    }

    public static void SetLanguage(GameLanguage language)
    {
        currentLanguage = language;
    }

    public static string GetLanguageDisplayName(GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.Russian:
                return "Русский";

            case GameLanguage.Spanish:
                return "Español";

            case GameLanguage.French:
                return "Français";

            case GameLanguage.German:
                return "Deutsch";

            case GameLanguage.Italian:
                return "Italiano";

            case GameLanguage.Chinese:
                return "中文";

            case GameLanguage.Japanese:
                return "日本語";

            case GameLanguage.Arabic:
                return "العربية";

            case GameLanguage.Hebrew:
                return "עברית";

            default:
                return "English";
        }
    }

    public static GameUiTextConfig GetDefaultUIText(GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.Russian:
                return DefaultRussianUiText;

            case GameLanguage.Spanish:
                return DefaultSpanishUiText;

            case GameLanguage.French:
                return DefaultFrenchUiText;

            case GameLanguage.German:
                return DefaultGermanUiText;

            case GameLanguage.Italian:
                return DefaultItalianUiText;

            case GameLanguage.Chinese:
                return DefaultChineseUiText;

            case GameLanguage.Japanese:
                return DefaultJapaneseUiText;

            case GameLanguage.Arabic:
                return DefaultArabicUiText;

            case GameLanguage.Hebrew:
                return DefaultHebrewUiText;

            default:
                return DefaultUiText;
        }
    }

    public static string GetDefaultNoMissionsText(GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.Russian:
                return "Нет миссий";

            case GameLanguage.Spanish:
                return "Sin misiones";

            case GameLanguage.French:
                return "Aucune mission";

            case GameLanguage.German:
                return "Keine Missionen";

            case GameLanguage.Italian:
                return "Nessuna missione";

            case GameLanguage.Chinese:
                return "没有任务";

            case GameLanguage.Japanese:
                return "ミッションなし";

            case GameLanguage.Arabic:
                return "لا توجد مهام";

            case GameLanguage.Hebrew:
                return "אין משימות";

            default:
                return ShuttleConfig.DefaultNoMissionsText;
        }
    }

    public static string GetText(
        string englishText,
        string russianText = "",
        string spanishText = "",
        string frenchText = "",
        string germanText = "",
        string italianText = "",
        string chineseText = "",
        string japaneseText = "",
        string arabicText = "",
        string hebrewText = "")
    {
        switch (CurrentLanguage)
        {
            case GameLanguage.Russian:
                return GetFirstText(russianText, englishText);

            case GameLanguage.Spanish:
                return GetFirstText(spanishText, englishText);

            case GameLanguage.French:
                return GetFirstText(frenchText, englishText);

            case GameLanguage.German:
                return GetFirstText(germanText, englishText);

            case GameLanguage.Italian:
                return GetFirstText(italianText, englishText);

            case GameLanguage.Chinese:
                return GetFirstText(chineseText, englishText);

            case GameLanguage.Japanese:
                return GetFirstText(japaneseText, englishText);

            case GameLanguage.Arabic:
                return GetFirstText(arabicText, englishText);

            case GameLanguage.Hebrew:
                return GetFirstText(hebrewText, englishText);

            default:
                return GetFirstText(englishText, russianText);
        }
    }

    private static string GetFirstText(string preferredText, string fallbackText)
    {
        return string.IsNullOrWhiteSpace(preferredText) ? fallbackText : preferredText;
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
