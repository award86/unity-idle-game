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
    public const string DefaultClaimOfflineButtonText = "Claim";
    public const string DefaultClaimOfflineX2ButtonText = "Claim X2";
    public const string DefaultBuyButtonText = "Buy";
    public const string DefaultBuildButtonText = "Build";
    public const string DefaultMaxButtonText = "Max";
    public const string DefaultSendButtonText = "Send";
    public const string DefaultLoadingButtonText = "Loading";
    public const string DefaultFlyingButtonText = "Flying";
    public const string DefaultMineButtonText = "Mine";
    public const string DefaultNewGameButtonText = "New Game";
    public const string DefaultUpgradeButtonText = "Upgrade";
    public const string DefaultBuildMenuButtonText = "Build";
    public const string DefaultMissionButtonText = "Mission";
    public const string DefaultProduceMetalButtonText = "Produce";
    public const string DefaultUpgradeAvailableButtonText = "Upgrade Available!";
    public const string DefaultBuildAvailableButtonText = "Build Available!";
    public const string DefaultLanguagesButtonText = "Languages";
    public const string DefaultEnglishLanguageButtonText = "English";
    public const string DefaultRussianLanguageButtonText = "Russian";
    public const string DefaultYesButtonText = "Yes";
    public const string DefaultNoButtonText = "No";
    public const string DefaultAreYouSureText = "Are you sure?";
    public const string DefaultUpgradesPanelTitleText = "Upgrades";
    public const string DefaultBuildingsPanelTitleText = "Buildings";
    public const string DefaultMinerTabText = "Miner";
    public const string DefaultPowerTabText = "Power";
    public const string DefaultFactoryTabText = "Factory";
    public const string DefaultPlatformTabText = "Platform";
    public const string DefaultShuttleTabText = "Shuttle";
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
    public const string DefaultNetworkErrorText = "Network error";
    public const string DefaultStartupLoadingText = "Loading";

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
    [SerializeField] private string claimOfflineButtonText = DefaultClaimOfflineButtonText;
    [SerializeField] private string claimOfflineX2ButtonText = DefaultClaimOfflineX2ButtonText;
    [SerializeField] private string buyButtonText = DefaultBuyButtonText;
    [SerializeField] private string buildButtonText = DefaultBuildButtonText;
    [SerializeField] private string maxButtonText = DefaultMaxButtonText;
    [SerializeField] private string sendButtonText = DefaultSendButtonText;
    [SerializeField] private string loadingButtonText = DefaultLoadingButtonText;
    [SerializeField] private string flyingButtonText = DefaultFlyingButtonText;
    [SerializeField] private string mineButtonText = DefaultMineButtonText;
    [SerializeField] private string newGameButtonText = DefaultNewGameButtonText;
    [SerializeField] private string upgradeButtonText = DefaultUpgradeButtonText;
    [SerializeField] private string buildMenuButtonText = DefaultBuildMenuButtonText;
    [SerializeField] private string missionButtonText = DefaultMissionButtonText;
    [SerializeField] private string produceMetalButtonText = DefaultProduceMetalButtonText;
    [SerializeField] private string upgradeAvailableButtonText = DefaultUpgradeAvailableButtonText;
    [SerializeField] private string buildAvailableButtonText = DefaultBuildAvailableButtonText;
    [SerializeField] private string languagesButtonText = DefaultLanguagesButtonText;
    [SerializeField] private string englishLanguageButtonText = DefaultEnglishLanguageButtonText;
    [SerializeField] private string russianLanguageButtonText = DefaultRussianLanguageButtonText;
    [SerializeField] private string yesButtonText = DefaultYesButtonText;
    [SerializeField] private string noButtonText = DefaultNoButtonText;
    [SerializeField] private string areYouSureText = DefaultAreYouSureText;

    [Header("Panels And Tabs")]
    [SerializeField] private string upgradesPanelTitleText = DefaultUpgradesPanelTitleText;
    [SerializeField] private string buildingsPanelTitleText = DefaultBuildingsPanelTitleText;
    [SerializeField] private string minerTabText = DefaultMinerTabText;
    [SerializeField] private string powerTabText = DefaultPowerTabText;
    [SerializeField] private string factoryTabText = DefaultFactoryTabText;
    [SerializeField] private string platformTabText = DefaultPlatformTabText;
    [SerializeField] private string shuttleTabText = DefaultShuttleTabText;

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

    [Header("Startup")]
    [SerializeField] private string networkErrorText = DefaultNetworkErrorText;
    [SerializeField] private string startupLoadingText = DefaultStartupLoadingText;

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
    public string ClaimOfflineButtonText => GetValue(claimOfflineButtonText, DefaultClaimOfflineButtonText);
    public string ClaimOfflineX2ButtonText => GetValue(claimOfflineX2ButtonText, DefaultClaimOfflineX2ButtonText);
    public string BuyButtonText => GetValue(buyButtonText, DefaultBuyButtonText);
    public string BuildButtonText => GetValue(buildButtonText, DefaultBuildButtonText);
    public string MaxButtonText => GetValue(maxButtonText, DefaultMaxButtonText);
    public string SendButtonText => GetValue(sendButtonText, DefaultSendButtonText);
    public string LoadingButtonText => GetValue(loadingButtonText, DefaultLoadingButtonText);
    public string FlyingButtonText => GetValue(flyingButtonText, DefaultFlyingButtonText);
    public string MineButtonText => GetValue(mineButtonText, DefaultMineButtonText);
    public string NewGameButtonText => GetValue(newGameButtonText, DefaultNewGameButtonText);
    public string UpgradeButtonText => GetValue(upgradeButtonText, DefaultUpgradeButtonText);
    public string BuildMenuButtonText => GetValue(buildMenuButtonText, DefaultBuildMenuButtonText);
    public string MissionButtonText => GetValue(missionButtonText, DefaultMissionButtonText);
    public string ProduceMetalButtonText => GetValue(produceMetalButtonText, DefaultProduceMetalButtonText);
    public string UpgradeAvailableButtonText => GetValue(upgradeAvailableButtonText, DefaultUpgradeAvailableButtonText);
    public string BuildAvailableButtonText => GetValue(buildAvailableButtonText, DefaultBuildAvailableButtonText);
    public string LanguagesButtonText => GetValue(languagesButtonText, DefaultLanguagesButtonText);
    public string EnglishLanguageButtonText => GetValue(englishLanguageButtonText, DefaultEnglishLanguageButtonText);
    public string RussianLanguageButtonText => GetValue(russianLanguageButtonText, DefaultRussianLanguageButtonText);
    public string YesButtonText => GetValue(yesButtonText, DefaultYesButtonText);
    public string NoButtonText => GetValue(noButtonText, DefaultNoButtonText);
    public string AreYouSureText => GetValue(areYouSureText, DefaultAreYouSureText);
    public string UpgradesPanelTitleText => GetValue(upgradesPanelTitleText, DefaultUpgradesPanelTitleText);
    public string BuildingsPanelTitleText => GetValue(buildingsPanelTitleText, DefaultBuildingsPanelTitleText);
    public string MinerTabText => GetValue(minerTabText, DefaultMinerTabText);
    public string PowerTabText => GetValue(powerTabText, DefaultPowerTabText);
    public string FactoryTabText => GetValue(factoryTabText, DefaultFactoryTabText);
    public string PlatformTabText => GetValue(platformTabText, DefaultPlatformTabText);
    public string ShuttleTabText => GetValue(shuttleTabText, DefaultShuttleTabText);
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
    public string NetworkErrorText => GetValue(networkErrorText, DefaultNetworkErrorText);
    public string StartupLoadingText => GetValue(startupLoadingText, DefaultStartupLoadingText);

    private static string GetValue(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    public static GameUiTextConfig CreateDefaults(GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.Russian:
                return CreateRussianDefaults();

            case GameLanguage.Spanish:
                return CreateSpanishDefaults();

            case GameLanguage.French:
                return CreateFrenchDefaults();

            case GameLanguage.German:
                return CreateGermanDefaults();

            case GameLanguage.Italian:
                return CreateItalianDefaults();

            case GameLanguage.Chinese:
                return CreateChineseDefaults();

            case GameLanguage.Japanese:
                return CreateJapaneseDefaults();

            case GameLanguage.Arabic:
                return CreateArabicDefaults();

            case GameLanguage.Hebrew:
                return CreateHebrewDefaults();

            default:
                return new GameUiTextConfig();
        }
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
            claimOfflineButtonText = "Забрать",
            claimOfflineX2ButtonText = "Забрать X2",
            buyButtonText = "Купить",
            buildButtonText = "Построить",
            maxButtonText = "Макс",
            sendButtonText = "Отправить",
            loadingButtonText = "Загрузка",
            flyingButtonText = "Полет",
            mineButtonText = "Добывать",
            newGameButtonText = "Новая игра",
            upgradeButtonText = "Улучшения",
            buildMenuButtonText = "Постройки",
            missionButtonText = "Миссии",
            produceMetalButtonText = "Произвести",
            upgradeAvailableButtonText = "Доступно улучшение!",
            buildAvailableButtonText = "Доступна постройка!",
            languagesButtonText = "Язык",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "Да",
            noButtonText = "Нет",
            areYouSureText = "Вы уверены?",
            upgradesPanelTitleText = "Улучшения",
            buildingsPanelTitleText = "Постройки",
            minerTabText = "Добыча",
            powerTabText = "Энергия",
            factoryTabText = "Завод",
            platformTabText = "Платформа",
            shuttleTabText = "Шаттл",
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

    private static GameUiTextConfig CreateSpanishDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "Mineral",
            energyLabel = "Energía",
            metalLabel = "Metal",
            crystalLabel = "Cristal",
            platformLabel = "Plataforma",
            shuttleLabel = "Lanzadera",
            orePerSecondLabel = "Mineral / seg",
            orePerClickLabel = "Mineral / clic",
            acceptButtonText = "Aceptar",
            exitButtonText = "Salir",
            claimRewardButtonText = "Reclamar recompensa",
            claimOfflineButtonText = "Reclamar",
            claimOfflineX2ButtonText = "Reclamar X2",
            buyButtonText = "Comprar",
            buildButtonText = "Construir",
            maxButtonText = "Máx",
            sendButtonText = "Enviar",
            loadingButtonText = "Cargando",
            flyingButtonText = "En vuelo",
            mineButtonText = "Minar",
            newGameButtonText = "Nueva partida",
            upgradeButtonText = "Mejoras",
            buildMenuButtonText = "Construcción",
            missionButtonText = "Misiones",
            produceMetalButtonText = "Producir",
            upgradeAvailableButtonText = "¡Mejora disponible!",
            buildAvailableButtonText = "¡Construcción disponible!",
            languagesButtonText = "Idiomas",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "Sí",
            noButtonText = "No",
            areYouSureText = "¿Estás seguro?",
            upgradesPanelTitleText = "Mejoras",
            buildingsPanelTitleText = "Construcciones",
            minerTabText = "Minería",
            powerTabText = "Energía",
            factoryTabText = "Fábrica",
            platformTabText = "Plataforma",
            shuttleTabText = "Lanzadera",
            missionLabel = "Misión",
            missionCompleteSuffix = "Completada",
            rewardLabel = "Recompensa",
            rewardReadyLabel = "Recompensa lista",
            metaBonusesHeaderText = "Bonos meta",
            unlockedTabsProgressLabel = "Pestañas desbloqueadas",
            maxedResearchProgressLabel = "Investigación completada",
            progressLabel = "Progreso",
            levelLabel = "Nivel",
            bonusLevelLabel = "Nivel de bono",
            costLabel = "Coste",
            effectLabel = "Efecto",
            noneText = "Ninguno",
            maxCostText = "MÁX",
            freeText = "Gratis",
            offlineWarehouseRewardPrefix = "Enviado al almacén sin conexión",
            offlineWarehouseRewardResourceSuffix = "mineral",
            orePerClickEffectText = "Mineral / clic",
            orePerSecondEffectText = "Mineral / seg",
            energyCapacityEffectText = "Capacidad de energía",
            energyRegenEffectText = "Regeneración de energía",
            energyIntervalEffectText = "Intervalo de energía",
            metalPerCraftEffectText = "Metal / fabricación",
            oreCraftCostEffectText = "Coste de fabricación en mineral",
            energyCraftCostEffectText = "Coste de fabricación en energía",
            platformCapacityEffectText = "Capacidad de plataforma",
            shuttleCapacityEffectText = "Capacidad de lanzadera",
            shuttleLoadingEffectText = "Carga de lanzadera",
            shuttleTravelEffectText = "Viaje de lanzadera",
            autoDispatchShuttleEffectText = "Autoenvío de lanzadera",
            shuttleCountEffectText = "Lanzadera",
            unknownText = "Desconocido",
            multiplierPrefixText = "x",
            secondsSuffixText = "s"
        };
    }

    private static GameUiTextConfig CreateFrenchDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "Minerai",
            energyLabel = "Énergie",
            metalLabel = "Métal",
            crystalLabel = "Cristal",
            platformLabel = "Plateforme",
            shuttleLabel = "Navette",
            orePerSecondLabel = "Minerai / s",
            orePerClickLabel = "Minerai / clic",
            acceptButtonText = "Accepter",
            exitButtonText = "Quitter",
            claimRewardButtonText = "Réclamer la récompense",
            claimOfflineButtonText = "Réclamer",
            claimOfflineX2ButtonText = "Réclamer X2",
            buyButtonText = "Acheter",
            buildButtonText = "Construire",
            maxButtonText = "Max",
            sendButtonText = "Envoyer",
            loadingButtonText = "Chargement",
            flyingButtonText = "En vol",
            mineButtonText = "Miner",
            newGameButtonText = "Nouvelle partie",
            upgradeButtonText = "Améliorations",
            buildMenuButtonText = "Construction",
            missionButtonText = "Missions",
            produceMetalButtonText = "Produire",
            upgradeAvailableButtonText = "Amélioration disponible !",
            buildAvailableButtonText = "Construction disponible !",
            languagesButtonText = "Langues",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "Oui",
            noButtonText = "Non",
            areYouSureText = "Êtes-vous sûr ?",
            upgradesPanelTitleText = "Améliorations",
            buildingsPanelTitleText = "Bâtiments",
            minerTabText = "Extraction",
            powerTabText = "Énergie",
            factoryTabText = "Usine",
            platformTabText = "Plateforme",
            shuttleTabText = "Navette",
            missionLabel = "Mission",
            missionCompleteSuffix = "Terminée",
            rewardLabel = "Récompense",
            rewardReadyLabel = "Récompense prête",
            metaBonusesHeaderText = "Bonus méta",
            unlockedTabsProgressLabel = "Onglets débloqués",
            maxedResearchProgressLabel = "Recherche terminée",
            progressLabel = "Progression",
            levelLabel = "Niveau",
            bonusLevelLabel = "Niveau du bonus",
            costLabel = "Coût",
            effectLabel = "Effet",
            noneText = "Aucun",
            maxCostText = "MAX",
            freeText = "Gratuit",
            offlineWarehouseRewardPrefix = "Envoyé à l'entrepôt hors ligne",
            offlineWarehouseRewardResourceSuffix = "minerai",
            orePerClickEffectText = "Minerai / clic",
            orePerSecondEffectText = "Minerai / s",
            energyCapacityEffectText = "Capacité d'énergie",
            energyRegenEffectText = "Régénération d'énergie",
            energyIntervalEffectText = "Intervalle d'énergie",
            metalPerCraftEffectText = "Métal / fabrication",
            oreCraftCostEffectText = "Coût de fabrication en minerai",
            energyCraftCostEffectText = "Coût de fabrication en énergie",
            platformCapacityEffectText = "Capacité de plateforme",
            shuttleCapacityEffectText = "Capacité de navette",
            shuttleLoadingEffectText = "Chargement de navette",
            shuttleTravelEffectText = "Trajet de navette",
            autoDispatchShuttleEffectText = "Envoi auto de navette",
            shuttleCountEffectText = "Navette",
            unknownText = "Inconnu",
            multiplierPrefixText = "x",
            secondsSuffixText = "s"
        };
    }

    private static GameUiTextConfig CreateGermanDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "Erz",
            energyLabel = "Energie",
            metalLabel = "Metall",
            crystalLabel = "Kristall",
            platformLabel = "Plattform",
            shuttleLabel = "Shuttle",
            orePerSecondLabel = "Erz / Sek",
            orePerClickLabel = "Erz / Klick",
            acceptButtonText = "Akzeptieren",
            exitButtonText = "Beenden",
            claimRewardButtonText = "Belohnung abholen",
            claimOfflineButtonText = "Abholen",
            claimOfflineX2ButtonText = "Abholen X2",
            buyButtonText = "Kaufen",
            buildButtonText = "Bauen",
            maxButtonText = "Max",
            sendButtonText = "Senden",
            loadingButtonText = "Laden",
            flyingButtonText = "Im Flug",
            mineButtonText = "Abbauen",
            newGameButtonText = "Neues Spiel",
            upgradeButtonText = "Upgrades",
            buildMenuButtonText = "Bauen",
            missionButtonText = "Missionen",
            produceMetalButtonText = "Produzieren",
            upgradeAvailableButtonText = "Upgrade verfügbar!",
            buildAvailableButtonText = "Bau verfügbar!",
            languagesButtonText = "Sprachen",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "Ja",
            noButtonText = "Nein",
            areYouSureText = "Bist du sicher?",
            upgradesPanelTitleText = "Upgrades",
            buildingsPanelTitleText = "Gebäude",
            minerTabText = "Bergbau",
            powerTabText = "Energie",
            factoryTabText = "Fabrik",
            platformTabText = "Plattform",
            shuttleTabText = "Shuttle",
            missionLabel = "Mission",
            missionCompleteSuffix = "Abgeschlossen",
            rewardLabel = "Belohnung",
            rewardReadyLabel = "Belohnung bereit",
            metaBonusesHeaderText = "Meta-Boni",
            unlockedTabsProgressLabel = "Freigeschaltete Tabs",
            maxedResearchProgressLabel = "Forschung abgeschlossen",
            progressLabel = "Fortschritt",
            levelLabel = "Level",
            bonusLevelLabel = "Bonuslevel",
            costLabel = "Kosten",
            effectLabel = "Effekt",
            noneText = "Keine",
            maxCostText = "MAX",
            freeText = "Gratis",
            offlineWarehouseRewardPrefix = "Offline zum Lager gesendet",
            offlineWarehouseRewardResourceSuffix = "Erz",
            orePerClickEffectText = "Erz / Klick",
            orePerSecondEffectText = "Erz / Sek",
            energyCapacityEffectText = "Energiekapazität",
            energyRegenEffectText = "Energieregeneration",
            energyIntervalEffectText = "Energieintervall",
            metalPerCraftEffectText = "Metall / Herstellung",
            oreCraftCostEffectText = "Erzkosten pro Herstellung",
            energyCraftCostEffectText = "Energiekosten pro Herstellung",
            platformCapacityEffectText = "Plattformkapazität",
            shuttleCapacityEffectText = "Shuttle-Kapazität",
            shuttleLoadingEffectText = "Shuttle-Beladung",
            shuttleTravelEffectText = "Shuttle-Flug",
            autoDispatchShuttleEffectText = "Auto-Shuttle-Versand",
            shuttleCountEffectText = "Shuttle",
            unknownText = "Unbekannt",
            multiplierPrefixText = "x",
            secondsSuffixText = "s"
        };
    }

    private static GameUiTextConfig CreateItalianDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "Minerale",
            energyLabel = "Energia",
            metalLabel = "Metallo",
            crystalLabel = "Cristallo",
            platformLabel = "Piattaforma",
            shuttleLabel = "Navetta",
            orePerSecondLabel = "Minerale / sec",
            orePerClickLabel = "Minerale / clic",
            acceptButtonText = "Accetta",
            exitButtonText = "Esci",
            claimRewardButtonText = "Ritira ricompensa",
            claimOfflineButtonText = "Ritira",
            claimOfflineX2ButtonText = "Ritira X2",
            buyButtonText = "Compra",
            buildButtonText = "Costruisci",
            maxButtonText = "Max",
            sendButtonText = "Invia",
            loadingButtonText = "Caricamento",
            flyingButtonText = "In volo",
            mineButtonText = "Estrai",
            newGameButtonText = "Nuova partita",
            upgradeButtonText = "Miglioramenti",
            buildMenuButtonText = "Costruzione",
            missionButtonText = "Missioni",
            produceMetalButtonText = "Produci",
            upgradeAvailableButtonText = "Miglioramento disponibile!",
            buildAvailableButtonText = "Costruzione disponibile!",
            languagesButtonText = "Lingue",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "Sì",
            noButtonText = "No",
            areYouSureText = "Sei sicuro?",
            upgradesPanelTitleText = "Miglioramenti",
            buildingsPanelTitleText = "Edifici",
            minerTabText = "Estrazione",
            powerTabText = "Energia",
            factoryTabText = "Fabbrica",
            platformTabText = "Piattaforma",
            shuttleTabText = "Navetta",
            missionLabel = "Missione",
            missionCompleteSuffix = "Completata",
            rewardLabel = "Ricompensa",
            rewardReadyLabel = "Ricompensa pronta",
            metaBonusesHeaderText = "Bonus meta",
            unlockedTabsProgressLabel = "Schede sbloccate",
            maxedResearchProgressLabel = "Ricerca completata",
            progressLabel = "Progresso",
            levelLabel = "Livello",
            bonusLevelLabel = "Livello bonus",
            costLabel = "Costo",
            effectLabel = "Effetto",
            noneText = "Nessuno",
            maxCostText = "MAX",
            freeText = "Gratis",
            offlineWarehouseRewardPrefix = "Inviato al magazzino offline",
            offlineWarehouseRewardResourceSuffix = "minerale",
            orePerClickEffectText = "Minerale / clic",
            orePerSecondEffectText = "Minerale / sec",
            energyCapacityEffectText = "Capacità energia",
            energyRegenEffectText = "Rigenerazione energia",
            energyIntervalEffectText = "Intervallo energia",
            metalPerCraftEffectText = "Metallo / produzione",
            oreCraftCostEffectText = "Costo produzione in minerale",
            energyCraftCostEffectText = "Costo produzione in energia",
            platformCapacityEffectText = "Capacità piattaforma",
            shuttleCapacityEffectText = "Capacità navetta",
            shuttleLoadingEffectText = "Carico navetta",
            shuttleTravelEffectText = "Viaggio navetta",
            autoDispatchShuttleEffectText = "Auto-invio navetta",
            shuttleCountEffectText = "Navetta",
            unknownText = "Sconosciuto",
            multiplierPrefixText = "x",
            secondsSuffixText = "s"
        };
    }

    private static GameUiTextConfig CreateChineseDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "矿石",
            energyLabel = "能量",
            metalLabel = "金属",
            crystalLabel = "水晶",
            platformLabel = "平台",
            shuttleLabel = "穿梭机",
            orePerSecondLabel = "矿石 / 秒",
            orePerClickLabel = "矿石 / 点击",
            acceptButtonText = "接受",
            exitButtonText = "退出",
            claimRewardButtonText = "领取奖励",
            claimOfflineButtonText = "领取",
            claimOfflineX2ButtonText = "领取 X2",
            buyButtonText = "购买",
            buildButtonText = "建造",
            maxButtonText = "最大",
            sendButtonText = "发送",
            loadingButtonText = "装载中",
            flyingButtonText = "飞行中",
            mineButtonText = "采矿",
            newGameButtonText = "新游戏",
            upgradeButtonText = "升级",
            buildMenuButtonText = "建造",
            missionButtonText = "任务",
            produceMetalButtonText = "生产",
            upgradeAvailableButtonText = "有可用升级！",
            buildAvailableButtonText = "有可用建筑！",
            languagesButtonText = "语言",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "是",
            noButtonText = "否",
            areYouSureText = "确定吗？",
            upgradesPanelTitleText = "升级",
            buildingsPanelTitleText = "建筑",
            minerTabText = "采矿",
            powerTabText = "电力",
            factoryTabText = "工厂",
            platformTabText = "平台",
            shuttleTabText = "穿梭机",
            missionLabel = "任务",
            missionCompleteSuffix = "完成",
            rewardLabel = "奖励",
            rewardReadyLabel = "奖励就绪",
            metaBonusesHeaderText = "元加成",
            unlockedTabsProgressLabel = "已解锁标签",
            maxedResearchProgressLabel = "研究已完成",
            progressLabel = "进度",
            levelLabel = "等级",
            bonusLevelLabel = "加成等级",
            costLabel = "花费",
            effectLabel = "效果",
            noneText = "无",
            maxCostText = "最大",
            freeText = "免费",
            offlineWarehouseRewardPrefix = "离线时已送往仓库",
            offlineWarehouseRewardResourceSuffix = "矿石",
            orePerClickEffectText = "矿石 / 点击",
            orePerSecondEffectText = "矿石 / 秒",
            energyCapacityEffectText = "能量上限",
            energyRegenEffectText = "能量恢复",
            energyIntervalEffectText = "能量间隔",
            metalPerCraftEffectText = "金属 / 制造",
            oreCraftCostEffectText = "矿石制造成本",
            energyCraftCostEffectText = "能量制造成本",
            platformCapacityEffectText = "平台容量",
            shuttleCapacityEffectText = "穿梭机容量",
            shuttleLoadingEffectText = "穿梭机装载",
            shuttleTravelEffectText = "穿梭机航行",
            autoDispatchShuttleEffectText = "自动派遣穿梭机",
            shuttleCountEffectText = "穿梭机",
            unknownText = "未知",
            multiplierPrefixText = "x",
            secondsSuffixText = "秒"
        };
    }

    private static GameUiTextConfig CreateJapaneseDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "鉱石",
            energyLabel = "エネルギー",
            metalLabel = "金属",
            crystalLabel = "クリスタル",
            platformLabel = "プラットフォーム",
            shuttleLabel = "シャトル",
            orePerSecondLabel = "鉱石 / 秒",
            orePerClickLabel = "鉱石 / クリック",
            acceptButtonText = "受け入れる",
            exitButtonText = "終了",
            claimRewardButtonText = "報酬を受け取る",
            claimOfflineButtonText = "受け取る",
            claimOfflineX2ButtonText = "受け取る X2",
            buyButtonText = "購入",
            buildButtonText = "建設",
            maxButtonText = "最大",
            sendButtonText = "送る",
            loadingButtonText = "積み込み中",
            flyingButtonText = "飛行中",
            mineButtonText = "採掘",
            newGameButtonText = "新しいゲーム",
            upgradeButtonText = "アップグレード",
            buildMenuButtonText = "建設",
            missionButtonText = "ミッション",
            produceMetalButtonText = "生産",
            upgradeAvailableButtonText = "アップグレード可能！",
            buildAvailableButtonText = "建設可能！",
            languagesButtonText = "言語",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "はい",
            noButtonText = "いいえ",
            areYouSureText = "よろしいですか？",
            upgradesPanelTitleText = "アップグレード",
            buildingsPanelTitleText = "建物",
            minerTabText = "採掘",
            powerTabText = "電力",
            factoryTabText = "工場",
            platformTabText = "プラットフォーム",
            shuttleTabText = "シャトル",
            missionLabel = "ミッション",
            missionCompleteSuffix = "完了",
            rewardLabel = "報酬",
            rewardReadyLabel = "報酬準備完了",
            metaBonusesHeaderText = "メタボーナス",
            unlockedTabsProgressLabel = "解除済みタブ",
            maxedResearchProgressLabel = "研究完了",
            progressLabel = "進捗",
            levelLabel = "レベル",
            bonusLevelLabel = "ボーナスレベル",
            costLabel = "コスト",
            effectLabel = "効果",
            noneText = "なし",
            maxCostText = "最大",
            freeText = "無料",
            offlineWarehouseRewardPrefix = "オフライン中に倉庫へ配送",
            offlineWarehouseRewardResourceSuffix = "鉱石",
            orePerClickEffectText = "鉱石 / クリック",
            orePerSecondEffectText = "鉱石 / 秒",
            energyCapacityEffectText = "エネルギー上限",
            energyRegenEffectText = "エネルギー回復",
            energyIntervalEffectText = "エネルギー間隔",
            metalPerCraftEffectText = "金属 / 製造",
            oreCraftCostEffectText = "鉱石製造コスト",
            energyCraftCostEffectText = "エネルギー製造コスト",
            platformCapacityEffectText = "プラットフォーム容量",
            shuttleCapacityEffectText = "シャトル容量",
            shuttleLoadingEffectText = "シャトル積み込み",
            shuttleTravelEffectText = "シャトル移動",
            autoDispatchShuttleEffectText = "シャトル自動派遣",
            shuttleCountEffectText = "シャトル",
            unknownText = "不明",
            multiplierPrefixText = "x",
            secondsSuffixText = "秒"
        };
    }

    private static GameUiTextConfig CreateArabicDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "خام",
            energyLabel = "طاقة",
            metalLabel = "معدن",
            crystalLabel = "بلورة",
            platformLabel = "منصة",
            shuttleLabel = "مكوك",
            orePerSecondLabel = "خام / ثانية",
            orePerClickLabel = "خام / نقرة",
            acceptButtonText = "قبول",
            exitButtonText = "خروج",
            claimRewardButtonText = "تحصيل المكافأة",
            claimOfflineButtonText = "تحصيل",
            claimOfflineX2ButtonText = "تحصيل X2",
            buyButtonText = "شراء",
            buildButtonText = "بناء",
            maxButtonText = "الحد الأقصى",
            sendButtonText = "إرسال",
            loadingButtonText = "جار التحميل",
            flyingButtonText = "في الرحلة",
            mineButtonText = "تعدين",
            newGameButtonText = "لعبة جديدة",
            upgradeButtonText = "ترقيات",
            buildMenuButtonText = "بناء",
            missionButtonText = "مهام",
            produceMetalButtonText = "إنتاج",
            upgradeAvailableButtonText = "ترقية متاحة!",
            buildAvailableButtonText = "بناء متاح!",
            languagesButtonText = "اللغات",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "نعم",
            noButtonText = "لا",
            areYouSureText = "هل أنت متأكد؟",
            upgradesPanelTitleText = "ترقيات",
            buildingsPanelTitleText = "مبانٍ",
            minerTabText = "تعدين",
            powerTabText = "طاقة",
            factoryTabText = "مصنع",
            platformTabText = "منصة",
            shuttleTabText = "مكوك",
            missionLabel = "مهمة",
            missionCompleteSuffix = "مكتملة",
            rewardLabel = "مكافأة",
            rewardReadyLabel = "المكافأة جاهزة",
            metaBonusesHeaderText = "مكافآت ميتا",
            unlockedTabsProgressLabel = "تبويبات مفتوحة",
            maxedResearchProgressLabel = "البحث مكتمل",
            progressLabel = "التقدم",
            levelLabel = "المستوى",
            bonusLevelLabel = "مستوى المكافأة",
            costLabel = "التكلفة",
            effectLabel = "التأثير",
            noneText = "لا شيء",
            maxCostText = "الأقصى",
            freeText = "مجاني",
            offlineWarehouseRewardPrefix = "أُرسل إلى المستودع أثناء عدم الاتصال",
            offlineWarehouseRewardResourceSuffix = "خام",
            orePerClickEffectText = "خام / نقرة",
            orePerSecondEffectText = "خام / ثانية",
            energyCapacityEffectText = "سعة الطاقة",
            energyRegenEffectText = "تجدد الطاقة",
            energyIntervalEffectText = "فاصل الطاقة",
            metalPerCraftEffectText = "معدن / تصنيع",
            oreCraftCostEffectText = "تكلفة التصنيع بالخام",
            energyCraftCostEffectText = "تكلفة التصنيع بالطاقة",
            platformCapacityEffectText = "سعة المنصة",
            shuttleCapacityEffectText = "سعة المكوك",
            shuttleLoadingEffectText = "تحميل المكوك",
            shuttleTravelEffectText = "رحلة المكوك",
            autoDispatchShuttleEffectText = "إرسال المكوك تلقائياً",
            shuttleCountEffectText = "مكوك",
            unknownText = "غير معروف",
            multiplierPrefixText = "x",
            secondsSuffixText = "ث"
        };
    }

    private static GameUiTextConfig CreateHebrewDefaults()
    {
        return new GameUiTextConfig
        {
            oreLabel = "עפרה",
            energyLabel = "אנרגיה",
            metalLabel = "מתכת",
            crystalLabel = "קריסטל",
            platformLabel = "פלטפורמה",
            shuttleLabel = "מעבורת",
            orePerSecondLabel = "עפרה / שנייה",
            orePerClickLabel = "עפרה / לחיצה",
            acceptButtonText = "קבל",
            exitButtonText = "יציאה",
            claimRewardButtonText = "קבל פרס",
            claimOfflineButtonText = "קבל",
            claimOfflineX2ButtonText = "קבל X2",
            buyButtonText = "קנה",
            buildButtonText = "בנה",
            maxButtonText = "מקסימום",
            sendButtonText = "שלח",
            loadingButtonText = "טוען",
            flyingButtonText = "בטיסה",
            mineButtonText = "כרייה",
            newGameButtonText = "משחק חדש",
            upgradeButtonText = "שדרוגים",
            buildMenuButtonText = "בנייה",
            missionButtonText = "משימות",
            produceMetalButtonText = "ייצר",
            upgradeAvailableButtonText = "שדרוג זמין!",
            buildAvailableButtonText = "בנייה זמינה!",
            languagesButtonText = "שפות",
            englishLanguageButtonText = "English",
            russianLanguageButtonText = "Русский",
            yesButtonText = "כן",
            noButtonText = "לא",
            areYouSureText = "האם אתה בטוח?",
            upgradesPanelTitleText = "שדרוגים",
            buildingsPanelTitleText = "מבנים",
            minerTabText = "כרייה",
            powerTabText = "אנרגיה",
            factoryTabText = "מפעל",
            platformTabText = "פלטפורמה",
            shuttleTabText = "מעבורת",
            missionLabel = "משימה",
            missionCompleteSuffix = "הושלמה",
            rewardLabel = "פרס",
            rewardReadyLabel = "הפרס מוכן",
            metaBonusesHeaderText = "בונוסי מטא",
            unlockedTabsProgressLabel = "לשוניות שנפתחו",
            maxedResearchProgressLabel = "המחקר הושלם",
            progressLabel = "התקדמות",
            levelLabel = "רמה",
            bonusLevelLabel = "רמת בונוס",
            costLabel = "עלות",
            effectLabel = "אפקט",
            noneText = "אין",
            maxCostText = "מקסימום",
            freeText = "חינם",
            offlineWarehouseRewardPrefix = "נשלח למחסן במצב לא מקוון",
            offlineWarehouseRewardResourceSuffix = "עפרה",
            orePerClickEffectText = "עפרה / לחיצה",
            orePerSecondEffectText = "עפרה / שנייה",
            energyCapacityEffectText = "קיבולת אנרגיה",
            energyRegenEffectText = "התחדשות אנרגיה",
            energyIntervalEffectText = "מרווח אנרגיה",
            metalPerCraftEffectText = "מתכת / יצירה",
            oreCraftCostEffectText = "עלות יצירה בעפרה",
            energyCraftCostEffectText = "עלות יצירה באנרגיה",
            platformCapacityEffectText = "קיבולת פלטפורמה",
            shuttleCapacityEffectText = "קיבולת מעבורת",
            shuttleLoadingEffectText = "טעינת מעבורת",
            shuttleTravelEffectText = "טיסת מעבורת",
            autoDispatchShuttleEffectText = "שליחה אוטומטית של מעבורת",
            shuttleCountEffectText = "מעבורת",
            unknownText = "לא ידוע",
            multiplierPrefixText = "x",
            secondsSuffixText = "ש"
        };
    }
}
