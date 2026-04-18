using UnityEngine;

[System.Serializable]
public class GameUiTextConfig
{
    public const string DefaultOreLabel = "Ore";
    public const string DefaultEnergyLabel = "Energy";
    public const string DefaultMetalLabel = "Metal";
    public const string DefaultCrystalLabel = "Crystal";
    public const string DefaultPlatformLabel = "Platform";
    public const string DefaultShuttleLabel = "Shuttle";
    public const string DefaultOrePerSecondLabel = "Ore / sec";
    public const string DefaultOrePerClickLabel = "Ore / click";
    public const string DefaultAcceptButtonText = "Accept";
    public const string DefaultExitButtonText = "Exit";
    public const string DefaultClaimRewardButtonText = "Claim Reward";
    public const string DefaultBuyButtonText = "Buy";
    public const string DefaultBuildButtonText = "Build";
    public const string DefaultMaxButtonText = "Max";
    public const string DefaultSendButtonText = "Send";
    public const string DefaultLoadingButtonText = "Loading";
    public const string DefaultFlyingButtonText = "Flying";
    public const string DefaultProduceMetalButtonText = "Produce";
    public const string DefaultUpgradeAvailableButtonText = "Upgrade Available!";
    public const string DefaultBuildAvailableButtonText = "Build Available!";
    public const string DefaultLanguageToggleButtonText = "Русский";
    public const string DefaultMissionLabel = "Mission";
    public const string DefaultMissionCompleteSuffix = "Complete";
    public const string DefaultRewardLabel = "Reward";
    public const string DefaultRewardReadyLabel = "Reward ready";
    public const string DefaultMetaBonusesHeaderText = "Meta Bonuses";
    public const string DefaultUnlockedTabsProgressLabel = "Unlocked tabs";
    public const string DefaultMaxedResearchProgressLabel = "Maxed research";
    public const string DefaultProgressLabel = "Progress";
    public const string DefaultLevelLabel = "Level";
    public const string DefaultBonusLevelLabel = "Bonus Level";
    public const string DefaultCostLabel = "Cost";
    public const string DefaultEffectLabel = "Effect";
    public const string DefaultNoneText = "None";
    public const string DefaultMaxCostText = "MAX";
    public const string DefaultFreeText = "Free";
    public const string DefaultOfflineWarehouseRewardPrefix = "Sent to warehouse while offline";
    public const string DefaultOfflineWarehouseRewardResourceSuffix = "Ore";
    public const string DefaultOrePerClickEffectText = "Ore / click";
    public const string DefaultOrePerSecondEffectText = "Ore / sec";
    public const string DefaultEnergyCapacityEffectText = "Energy cap";
    public const string DefaultEnergyRegenEffectText = "Energy regen";
    public const string DefaultEnergyIntervalEffectText = "Energy interval";
    public const string DefaultMetalPerCraftEffectText = "Metal / craft";
    public const string DefaultOreCraftCostEffectText = "Ore craft cost";
    public const string DefaultEnergyCraftCostEffectText = "Energy craft cost";
    public const string DefaultPlatformCapacityEffectText = "Platform capacity";
    public const string DefaultShuttleCapacityEffectText = "Shuttle capacity";
    public const string DefaultShuttleLoadingEffectText = "Shuttle loading";
    public const string DefaultShuttleTravelEffectText = "Shuttle travel";
    public const string DefaultAutoDispatchShuttleEffectText = "Auto-dispatch shuttle";
    public const string DefaultShuttleCountEffectText = "Shuttle";
    public const string DefaultUnknownText = "Unknown";
    public const string DefaultMultiplierPrefixText = "x";
    public const string DefaultSecondsSuffixText = "s";

    [Header("HUD Labels")]
    [SerializeField] private string oreLabel = DefaultOreLabel;
    [SerializeField] private string energyLabel = DefaultEnergyLabel;
    [SerializeField] private string metalLabel = DefaultMetalLabel;
    [SerializeField] private string crystalLabel = DefaultCrystalLabel;
    [SerializeField] private string platformLabel = DefaultPlatformLabel;
    [SerializeField] private string shuttleLabel = DefaultShuttleLabel;
    [SerializeField] private string orePerSecondLabel = DefaultOrePerSecondLabel;
    [SerializeField] private string orePerClickLabel = DefaultOrePerClickLabel;

    [Header("Buttons")]
    [SerializeField] private string acceptButtonText = DefaultAcceptButtonText;
    [SerializeField] private string exitButtonText = DefaultExitButtonText;
    [SerializeField] private string claimRewardButtonText = DefaultClaimRewardButtonText;
    [SerializeField] private string buyButtonText = DefaultBuyButtonText;
    [SerializeField] private string buildButtonText = DefaultBuildButtonText;
    [SerializeField] private string maxButtonText = DefaultMaxButtonText;
    [SerializeField] private string sendButtonText = DefaultSendButtonText;
    [SerializeField] private string loadingButtonText = DefaultLoadingButtonText;
    [SerializeField] private string flyingButtonText = DefaultFlyingButtonText;
    [SerializeField] private string produceMetalButtonText = DefaultProduceMetalButtonText;
    [SerializeField] private string upgradeAvailableButtonText = DefaultUpgradeAvailableButtonText;
    [SerializeField] private string buildAvailableButtonText = DefaultBuildAvailableButtonText;
    [SerializeField] private string languageToggleButtonText = DefaultLanguageToggleButtonText;

    [Header("Mission")]
    [SerializeField] private string missionLabel = DefaultMissionLabel;
    [SerializeField] private string missionCompleteSuffix = DefaultMissionCompleteSuffix;
    [SerializeField] private string rewardLabel = DefaultRewardLabel;
    [SerializeField] private string rewardReadyLabel = DefaultRewardReadyLabel;
    [SerializeField] private string metaBonusesHeaderText = DefaultMetaBonusesHeaderText;
    [SerializeField] private string unlockedTabsProgressLabel = DefaultUnlockedTabsProgressLabel;
    [SerializeField] private string maxedResearchProgressLabel = DefaultMaxedResearchProgressLabel;
    [SerializeField] private string progressLabel = DefaultProgressLabel;

    [Header("Common")]
    [SerializeField] private string levelLabel = DefaultLevelLabel;
    [SerializeField] private string bonusLevelLabel = DefaultBonusLevelLabel;
    [SerializeField] private string costLabel = DefaultCostLabel;
    [SerializeField] private string effectLabel = DefaultEffectLabel;
    [SerializeField] private string noneText = DefaultNoneText;
    [SerializeField] private string maxCostText = DefaultMaxCostText;
    [SerializeField] private string freeText = DefaultFreeText;

    [Header("Offline")]
    [SerializeField] private string offlineWarehouseRewardPrefix = DefaultOfflineWarehouseRewardPrefix;
    [SerializeField] private string offlineWarehouseRewardResourceSuffix = DefaultOfflineWarehouseRewardResourceSuffix;

    [Header("Effects")]
    [SerializeField] private string orePerClickEffectText = DefaultOrePerClickEffectText;
    [SerializeField] private string orePerSecondEffectText = DefaultOrePerSecondEffectText;
    [SerializeField] private string energyCapacityEffectText = DefaultEnergyCapacityEffectText;
    [SerializeField] private string energyRegenEffectText = DefaultEnergyRegenEffectText;
    [SerializeField] private string energyIntervalEffectText = DefaultEnergyIntervalEffectText;
    [SerializeField] private string metalPerCraftEffectText = DefaultMetalPerCraftEffectText;
    [SerializeField] private string oreCraftCostEffectText = DefaultOreCraftCostEffectText;
    [SerializeField] private string energyCraftCostEffectText = DefaultEnergyCraftCostEffectText;
    [SerializeField] private string platformCapacityEffectText = DefaultPlatformCapacityEffectText;
    [SerializeField] private string shuttleCapacityEffectText = DefaultShuttleCapacityEffectText;
    [SerializeField] private string shuttleLoadingEffectText = DefaultShuttleLoadingEffectText;
    [SerializeField] private string shuttleTravelEffectText = DefaultShuttleTravelEffectText;
    [SerializeField] private string autoDispatchShuttleEffectText = DefaultAutoDispatchShuttleEffectText;
    [SerializeField] private string shuttleCountEffectText = DefaultShuttleCountEffectText;
    [SerializeField] private string unknownText = DefaultUnknownText;
    [SerializeField] private string multiplierPrefixText = DefaultMultiplierPrefixText;
    [SerializeField] private string secondsSuffixText = DefaultSecondsSuffixText;

    public string OreLabel => GetValue(oreLabel, DefaultOreLabel);
    public string EnergyLabel => GetValue(energyLabel, DefaultEnergyLabel);
    public string MetalLabel => GetValue(metalLabel, DefaultMetalLabel);
    public string CrystalLabel => GetValue(crystalLabel, DefaultCrystalLabel);
    public string PlatformLabel => GetValue(platformLabel, DefaultPlatformLabel);
    public string ShuttleLabel => GetValue(shuttleLabel, DefaultShuttleLabel);
    public string OrePerSecondLabel => GetValue(orePerSecondLabel, DefaultOrePerSecondLabel);
    public string OrePerClickLabel => GetValue(orePerClickLabel, DefaultOrePerClickLabel);
    public string AcceptButtonText => GetValue(acceptButtonText, DefaultAcceptButtonText);
    public string ExitButtonText => GetValue(exitButtonText, DefaultExitButtonText);
    public string ClaimRewardButtonText => GetValue(claimRewardButtonText, DefaultClaimRewardButtonText);
    public string BuyButtonText => GetValue(buyButtonText, DefaultBuyButtonText);
    public string BuildButtonText => GetValue(buildButtonText, DefaultBuildButtonText);
    public string MaxButtonText => GetValue(maxButtonText, DefaultMaxButtonText);
    public string SendButtonText => GetValue(sendButtonText, DefaultSendButtonText);
    public string LoadingButtonText => GetValue(loadingButtonText, DefaultLoadingButtonText);
    public string FlyingButtonText => GetValue(flyingButtonText, DefaultFlyingButtonText);
    public string ProduceMetalButtonText => GetValue(produceMetalButtonText, DefaultProduceMetalButtonText);
    public string UpgradeAvailableButtonText => GetValue(upgradeAvailableButtonText, DefaultUpgradeAvailableButtonText);
    public string BuildAvailableButtonText => GetValue(buildAvailableButtonText, DefaultBuildAvailableButtonText);
    public string LanguageToggleButtonText => GetValue(languageToggleButtonText, DefaultLanguageToggleButtonText);
    public string MissionLabel => GetValue(missionLabel, DefaultMissionLabel);
    public string MissionCompleteSuffix => GetValue(missionCompleteSuffix, DefaultMissionCompleteSuffix);
    public string RewardLabel => GetValue(rewardLabel, DefaultRewardLabel);
    public string RewardReadyLabel => GetValue(rewardReadyLabel, DefaultRewardReadyLabel);
    public string MetaBonusesHeaderText => GetValue(metaBonusesHeaderText, DefaultMetaBonusesHeaderText);
    public string UnlockedTabsProgressLabel => GetValue(unlockedTabsProgressLabel, DefaultUnlockedTabsProgressLabel);
    public string MaxedResearchProgressLabel => GetValue(maxedResearchProgressLabel, DefaultMaxedResearchProgressLabel);
    public string ProgressLabel => GetValue(progressLabel, DefaultProgressLabel);
    public string LevelLabel => GetValue(levelLabel, DefaultLevelLabel);
    public string BonusLevelLabel => GetValue(bonusLevelLabel, DefaultBonusLevelLabel);
    public string CostLabel => GetValue(costLabel, DefaultCostLabel);
    public string EffectLabel => GetValue(effectLabel, DefaultEffectLabel);
    public string NoneText => GetValue(noneText, DefaultNoneText);
    public string MaxCostText => GetValue(maxCostText, DefaultMaxCostText);
    public string FreeText => GetValue(freeText, DefaultFreeText);
    public string OfflineWarehouseRewardPrefix => GetValue(offlineWarehouseRewardPrefix, DefaultOfflineWarehouseRewardPrefix);
    public string OfflineWarehouseRewardResourceSuffix => GetValue(offlineWarehouseRewardResourceSuffix, DefaultOfflineWarehouseRewardResourceSuffix);
    public string OrePerClickEffectText => GetValue(orePerClickEffectText, DefaultOrePerClickEffectText);
    public string OrePerSecondEffectText => GetValue(orePerSecondEffectText, DefaultOrePerSecondEffectText);
    public string EnergyCapacityEffectText => GetValue(energyCapacityEffectText, DefaultEnergyCapacityEffectText);
    public string EnergyRegenEffectText => GetValue(energyRegenEffectText, DefaultEnergyRegenEffectText);
    public string EnergyIntervalEffectText => GetValue(energyIntervalEffectText, DefaultEnergyIntervalEffectText);
    public string MetalPerCraftEffectText => GetValue(metalPerCraftEffectText, DefaultMetalPerCraftEffectText);
    public string OreCraftCostEffectText => GetValue(oreCraftCostEffectText, DefaultOreCraftCostEffectText);
    public string EnergyCraftCostEffectText => GetValue(energyCraftCostEffectText, DefaultEnergyCraftCostEffectText);
    public string PlatformCapacityEffectText => GetValue(platformCapacityEffectText, DefaultPlatformCapacityEffectText);
    public string ShuttleCapacityEffectText => GetValue(shuttleCapacityEffectText, DefaultShuttleCapacityEffectText);
    public string ShuttleLoadingEffectText => GetValue(shuttleLoadingEffectText, DefaultShuttleLoadingEffectText);
    public string ShuttleTravelEffectText => GetValue(shuttleTravelEffectText, DefaultShuttleTravelEffectText);
    public string AutoDispatchShuttleEffectText => GetValue(autoDispatchShuttleEffectText, DefaultAutoDispatchShuttleEffectText);
    public string ShuttleCountEffectText => GetValue(shuttleCountEffectText, DefaultShuttleCountEffectText);
    public string UnknownText => GetValue(unknownText, DefaultUnknownText);
    public string MultiplierPrefixText => GetValue(multiplierPrefixText, DefaultMultiplierPrefixText);
    public string SecondsSuffixText => GetValue(secondsSuffixText, DefaultSecondsSuffixText);

    private static string GetValue(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    public static GameUiTextConfig CreateRussianDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "Руда",
            energyLabel = "Энергия",
            metalLabel = "Металл",
            crystalLabel = "Кристалл",
            platformLabel = "Платформа",
            shuttleLabel = "Шаттл",
            orePerSecondLabel = "Руда / сек",
            orePerClickLabel = "Руда / клик",
            acceptButtonText = "Принять",
            exitButtonText = "Выход",
            claimRewardButtonText = "Забрать награду",
            buyButtonText = "Купить",
            buildButtonText = "Построить",
            maxButtonText = "Макс",
            sendButtonText = "Отправить",
            loadingButtonText = "Загрузка",
            flyingButtonText = "Полет",
            produceMetalButtonText = "Произвести",
            upgradeAvailableButtonText = "Доступно улучшение!",
            buildAvailableButtonText = "Доступна постройка!",
            languageToggleButtonText = "English",
            missionLabel = "Миссия",
            missionCompleteSuffix = "Завершена",
            rewardLabel = "Награда",
            rewardReadyLabel = "Награда готова",
            metaBonusesHeaderText = "Мета-бонусы",
            unlockedTabsProgressLabel = "Открыто вкладок",
            maxedResearchProgressLabel = "Исследовано",
            progressLabel = "Прогресс",
            levelLabel = "Уровень",
            bonusLevelLabel = "Уровень бонуса",
            costLabel = "Стоимость",
            effectLabel = "Эффект",
            noneText = "Нет",
            maxCostText = "МАКС",
            freeText = "Бесплатно",
            offlineWarehouseRewardPrefix = "Доставлено на склад офлайн",
            offlineWarehouseRewardResourceSuffix = "руды",
            orePerClickEffectText = "Руда / клик",
            orePerSecondEffectText = "Руда / сек",
            energyCapacityEffectText = "Лимит энергии",
            energyRegenEffectText = "Реген энергии",
            energyIntervalEffectText = "Интервал энергии",
            metalPerCraftEffectText = "Металл / крафт",
            oreCraftCostEffectText = "Стоимость крафта в руде",
            energyCraftCostEffectText = "Стоимость крафта в энергии",
            platformCapacityEffectText = "Вместимость платформы",
            shuttleCapacityEffectText = "Вместимость шаттла",
            shuttleLoadingEffectText = "Загрузка шаттла",
            shuttleTravelEffectText = "Полет шаттла",
            autoDispatchShuttleEffectText = "Автоотправка шаттла",
            shuttleCountEffectText = "Шаттл",
            unknownText = "Неизвестно",
            multiplierPrefixText = "x",
            secondsSuffixText = "с"
        };
    }
}
