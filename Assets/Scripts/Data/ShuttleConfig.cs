using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Idle Space/Game Config")]
public class ShuttleConfig : ScriptableObject
{
    public const int DefaultStartOre = 0;
    public const int DefaultStartEnergy = 0;
    public const int DefaultStartMetal = 0;
    public const int DefaultStartCrystal = 0;
    public const int DefaultStartOrePerClick = 1;
    public const int DefaultStartOrePerSecond = 0;
    public const int DefaultStartEnergyMax = 10;
    public const int DefaultStartEnergyRegenAmount = 1;
    public const float DefaultStartEnergyRegenInterval = 5f;
    public const float DefaultMinEnergyRegenInterval = 1f;
    public const int DefaultMetalPerCraft = 1;
    public const int DefaultMetalOreCost = 25;
    public const int DefaultMetalEnergyCost = 3;
    public const int DefaultOrePerCrystal = 2500;
    public const int DefaultMetalPerCrystal = 100;
    public const int DefaultPlatformCapacity = 30;
    public const int DefaultCapacity = 20;
    public const float DefaultLoadingTimeSeconds = 2f;
    public const float DefaultTravelTimeSeconds = 10f;
    public const float DefaultBoostOfferAutoCloseSeconds = 120f;
    public const string DefaultNoMissionsText = "No missions";

    [Header("Resources")]
    [SerializeField] private int startOreAmount = DefaultStartOre;
    [SerializeField] private int startEnergy = DefaultStartEnergy;
    [SerializeField] private int startMetal = DefaultStartMetal;
    [SerializeField] private int startCrystal = DefaultStartCrystal;
    [SerializeField] private int startOrePerClick = DefaultStartOrePerClick;
    [SerializeField] private int startOrePerSecond = DefaultStartOrePerSecond;

    [Header("Energy")]
    [SerializeField] private int startEnergyMax = DefaultStartEnergyMax;
    [SerializeField] private int startEnergyRegenAmount = DefaultStartEnergyRegenAmount;
    [SerializeField] private float startEnergyRegenInterval = DefaultStartEnergyRegenInterval;
    [SerializeField] private float minEnergyRegenInterval = DefaultMinEnergyRegenInterval;

    [Header("Economy")]
    [SerializeField] private int metalPerCraft = DefaultMetalPerCraft;
    [SerializeField] private int metalOreCost = DefaultMetalOreCost;
    [SerializeField] private int metalEnergyCost = DefaultMetalEnergyCost;
    [SerializeField] private int orePerCrystal = DefaultOrePerCrystal;
    [SerializeField] private int metalPerCrystal = DefaultMetalPerCrystal;

    [Header("Shuttle")]
    [SerializeField] private int startPlatformCapacity = DefaultPlatformCapacity;
    [FormerlySerializedAs("startOre")]
    [SerializeField] private int shuttleStartOre = DefaultStartOre;
    [FormerlySerializedAs("capacity")]
    [SerializeField] private int shuttleCapacity = DefaultCapacity;
    [SerializeField] private float shuttleLoadingTimeSeconds = DefaultLoadingTimeSeconds;
    [FormerlySerializedAs("travelTimeSeconds")]
    [SerializeField] private float shuttleTravelTimeSeconds = DefaultTravelTimeSeconds;

    [Header("Boost Offer")]
    [SerializeField] private float boostOfferAutoCloseSeconds = DefaultBoostOfferAutoCloseSeconds;

    [Header("UI")]
    [SerializeField] private string noMissionsText = DefaultNoMissionsText;
    [SerializeField] private string russianNoMissionsText = "Нет миссий";
    [SerializeField] private string spanishNoMissionsText = "Sin misiones";
    [SerializeField] private string frenchNoMissionsText = "Aucune mission";
    [SerializeField] private string germanNoMissionsText = "Keine Missionen";
    [SerializeField] private string italianNoMissionsText = "Nessuna missione";
    [SerializeField] private string chineseNoMissionsText = "没有任务";
    [SerializeField] private string japaneseNoMissionsText = "ミッションなし";
    [SerializeField] private string arabicNoMissionsText = "لا توجد مهام";
    [SerializeField] private string hebrewNoMissionsText = "אין משימות";
    [SerializeField] private GameUiTextConfig uiText = new GameUiTextConfig();
    [SerializeField] private GameUiTextConfig russianUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Russian);
    [SerializeField] private GameUiTextConfig spanishUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Spanish);
    [SerializeField] private GameUiTextConfig frenchUiText = GameUiTextConfig.CreateDefaults(GameLanguage.French);
    [SerializeField] private GameUiTextConfig germanUiText = GameUiTextConfig.CreateDefaults(GameLanguage.German);
    [SerializeField] private GameUiTextConfig italianUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Italian);
    [SerializeField] private GameUiTextConfig chineseUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Chinese);
    [SerializeField] private GameUiTextConfig japaneseUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Japanese);
    [SerializeField] private GameUiTextConfig arabicUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Arabic);
    [SerializeField] private GameUiTextConfig hebrewUiText = GameUiTextConfig.CreateDefaults(GameLanguage.Hebrew);

    public int StartOre => Mathf.Max(0, startOreAmount);
    public int StartEnergy => Mathf.Max(0, startEnergy);
    public int StartMetal => Mathf.Max(0, startMetal);
    public int StartCrystal => Mathf.Max(0, startCrystal);
    public int StartOrePerClick => Mathf.Max(0, startOrePerClick);
    public int StartOrePerSecond => Mathf.Max(0, startOrePerSecond);

    public int StartEnergyMax => Mathf.Max(1, startEnergyMax);
    public int StartEnergyRegenAmount => Mathf.Max(0, startEnergyRegenAmount);
    public float StartEnergyRegenInterval => Mathf.Max(0.1f, startEnergyRegenInterval);
    public float MinEnergyRegenInterval => Mathf.Max(0.1f, minEnergyRegenInterval);

    public int MetalPerCraft => Mathf.Max(1, metalPerCraft);
    public int MetalOreCost => Mathf.Max(0, metalOreCost);
    public int MetalEnergyCost => Mathf.Max(0, metalEnergyCost);
    public int OrePerCrystal => Mathf.Max(1, orePerCrystal);
    public int MetalPerCrystal => Mathf.Max(1, metalPerCrystal);

    public int StartPlatformCapacity => Mathf.Max(1, startPlatformCapacity);
    public int ShuttleStartOre => Mathf.Max(0, shuttleStartOre);
    public int Capacity => Mathf.Max(1, shuttleCapacity);
    public float LoadingTimeSeconds => Mathf.Max(0f, shuttleLoadingTimeSeconds);
    public float TravelTimeSeconds => Mathf.Max(0f, shuttleTravelTimeSeconds);
    public float BoostOfferAutoCloseSeconds => Mathf.Max(0f, boostOfferAutoCloseSeconds);
    public string NoMissionsText => string.IsNullOrWhiteSpace(noMissionsText) ? DefaultNoMissionsText : noMissionsText;
    public string RussianNoMissionsText => GetNoMissionsText(GameLanguage.Russian);
    public GameUiTextConfig UIText => uiText ?? (uiText = new GameUiTextConfig());
    public GameUiTextConfig RussianUIText => GetUIText(GameLanguage.Russian);

    public string GetNoMissionsText(GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.Russian:
                return GetTextOrDefault(russianNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            case GameLanguage.Spanish:
                return GetTextOrDefault(spanishNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            case GameLanguage.French:
                return GetTextOrDefault(frenchNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            case GameLanguage.German:
                return GetTextOrDefault(germanNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            case GameLanguage.Italian:
                return GetTextOrDefault(italianNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            case GameLanguage.Chinese:
                return GetTextOrDefault(chineseNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            case GameLanguage.Japanese:
                return GetTextOrDefault(japaneseNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            case GameLanguage.Arabic:
                return GetTextOrDefault(arabicNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            case GameLanguage.Hebrew:
                return GetTextOrDefault(hebrewNoMissionsText, GameTextProvider.GetDefaultNoMissionsText(language));

            default:
                return NoMissionsText;
        }
    }

    public GameUiTextConfig GetUIText(GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.Russian:
                return russianUiText ?? (russianUiText = GameUiTextConfig.CreateDefaults(language));

            case GameLanguage.Spanish:
                return spanishUiText ?? (spanishUiText = GameUiTextConfig.CreateDefaults(language));

            case GameLanguage.French:
                return frenchUiText ?? (frenchUiText = GameUiTextConfig.CreateDefaults(language));

            case GameLanguage.German:
                return germanUiText ?? (germanUiText = GameUiTextConfig.CreateDefaults(language));

            case GameLanguage.Italian:
                return italianUiText ?? (italianUiText = GameUiTextConfig.CreateDefaults(language));

            case GameLanguage.Chinese:
                return chineseUiText ?? (chineseUiText = GameUiTextConfig.CreateDefaults(language));

            case GameLanguage.Japanese:
                return japaneseUiText ?? (japaneseUiText = GameUiTextConfig.CreateDefaults(language));

            case GameLanguage.Arabic:
                return arabicUiText ?? (arabicUiText = GameUiTextConfig.CreateDefaults(language));

            case GameLanguage.Hebrew:
                return hebrewUiText ?? (hebrewUiText = GameUiTextConfig.CreateDefaults(language));

            default:
                return UIText;
        }
    }

    private static string GetTextOrDefault(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
