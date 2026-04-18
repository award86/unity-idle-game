using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text oreText;
    [SerializeField] private Text energyText;
    [SerializeField] private Text metalText;
    [SerializeField] private Text crystalText;
    [SerializeField] private Text platformText;
    [SerializeField] private Text shuttleText;
    [SerializeField] private Text shuttleText2;
    [SerializeField] private Text shuttleText3;
    [SerializeField] private Text orePerSecondText;

    [FormerlySerializedAs("upgradeCostText")]
    [SerializeField] private Text orePerClickText;
    [SerializeField] private Image energyFillImage;
    [SerializeField] private Text energyBarText;
    [SerializeField] private float energyDisplaySpeed = 8f;
    [SerializeField] private Image platformFillImage;
    [SerializeField] private Text platformBarText;
    [SerializeField] private Image shuttleFillImage;
    [SerializeField] private Text shuttleBarText;
    [SerializeField] private Image shuttleFillImage2;
    [SerializeField] private Text shuttleBarText2;
    [SerializeField] private Image shuttleFillImage3;
    [SerializeField] private Text shuttleBarText3;
    [SerializeField] private float safeAreaExtraTopPadding = 24f;
    [SerializeField] private float safeAreaExtraSidePadding = 12f;
    [SerializeField] private float centeredPopupYOffsetFactor = 0.35f;
    [SerializeField] private int upgradeContentTopPadding = 150;
    [SerializeField] private int buildContentTopPadding = 20;
    [SerializeField] private Button sendShuttleButton;
    [SerializeField] private Text sendShuttleButtonText;
    [SerializeField] private Button sendShuttleButton2;
    [SerializeField] private Text sendShuttleButtonText2;
    [SerializeField] private Button sendShuttleButton3;
    [SerializeField] private Text sendShuttleButtonText3;
    [SerializeField] private Button produceMetalButton;
    [SerializeField] private Text produceMetalButtonText;

    [FormerlySerializedAs("menuOverlayPanel")]
    [FormerlySerializedAs("boostOfferOverlayPanel")]
    [SerializeField] private GameObject sharedOverlayPanel;
    [SerializeField] private GameObject dropdownMenuPanel;
    [SerializeField] private Button exitMenuButton;
    [SerializeField] private Text exitMenuButtonText;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private GameObject mainScreenUpgradeButton;
    [SerializeField] private GameObject mainScreenBuildButton;
    [SerializeField] private GameObject mainScreenMissionButton;
    [SerializeField] private Button minerTabButton;
    [SerializeField] private Button platformTabButton;
    [SerializeField] private Button powerTabButton;
    [SerializeField] private Button factoryTabButton;
    [SerializeField] private Button shuttleTabButton;
    [SerializeField] private Transform upgradeListRoot;
    [SerializeField] private Transform buildListRoot;
    [SerializeField] private Transform metaBonusListRoot;
    [SerializeField] private UpgradeItemUI upgradeItemPrefab;
    [SerializeField] private BuildingItemUI buildingItemPrefab;
    [SerializeField] private MetaBonusItemUI metaBonusItemPrefab;
    [SerializeField] private Text boostStatusText;
    [SerializeField] private Text missionStatusText;
    [SerializeField] private Text missionTitleText;
    [SerializeField] private Text missionDescriptionText;
    [SerializeField] private Text missionProgressText;
    [SerializeField] private Text missionRewardText;
    [SerializeField] private Text metaBonusHeaderText;
    [SerializeField] private Button missionClaimButton;
    [SerializeField] private Text missionClaimButtonText;
    [SerializeField] private GameObject resetConfirmationPanel;
    [SerializeField] private GameObject offlineRewardPanel;
    [SerializeField] private Text offlineRewardText;
    [SerializeField] private Button boostOfferButton;
    [SerializeField] private Text boostOfferButtonText;

    private readonly List<UpgradeItemUI> upgradeItems = new List<UpgradeItemUI>();
    private readonly List<BuildingItemUI> buildingItems = new List<BuildingItemUI>();
    private readonly List<MetaBonusItemUI> metaBonusItems = new List<MetaBonusItemUI>();
    private readonly List<Button> shuttleButtons = new List<Button>();
    private readonly List<Text> shuttleButtonTexts = new List<Text>();
    private readonly List<Button> boostOfferButtons = new List<Button>();
    private readonly List<Text> boostOfferButtonTexts = new List<Text>();
    private readonly List<RectTransform> shuttleBarRoots = new List<RectTransform>();
    private readonly List<Image> shuttleFillImages = new List<Image>();
    private readonly List<Text> shuttleBarTexts = new List<Text>();
    private readonly Dictionary<RectTransform, Vector2> baseAnchoredPositions = new Dictionary<RectTransform, Vector2>();
    private readonly Dictionary<RectTransform, Vector2> baseOffsetMins = new Dictionary<RectTransform, Vector2>();
    private readonly Dictionary<RectTransform, Vector2> baseOffsetMaxes = new Dictionary<RectTransform, Vector2>();
    private readonly Dictionary<VerticalLayoutGroup, RectOffset> baseLayoutPaddings = new Dictionary<VerticalLayoutGroup, RectOffset>();
    private GameData lastDisplayedGameData;
    private UpgradeCategory selectedUpgradeCategory = UpgradeCategory.Miner;
    private float displayedEnergyAmount;
    private bool hasDisplayedEnergyAmount;
    private Canvas rootCanvas;
    private Rect lastAppliedSafeArea;
    private Vector2Int lastAppliedScreenSize;
    private RectTransform cachedMenuButtonRect;
    private Action<TemporaryBoostState> boostOfferAcceptHandler;

    public bool IsOfflineRewardVisible => offlineRewardPanel != null && offlineRewardPanel.activeSelf;
    public bool IsBoostOfferVisible => HasVisibleBoostOfferButtons();
    public bool IsUpgradePanelVisible => upgradePanel != null && upgradePanel.activeSelf;
    public bool IsBuildPanelVisible => buildPanel != null && buildPanel.activeSelf;
    public bool IsMissionPanelVisible => missionPanel != null && missionPanel.activeSelf;
    public bool IsMenuVisible => dropdownMenuPanel != null && dropdownMenuPanel.activeSelf;
    public bool IsResetConfirmationVisible => resetConfirmationPanel != null && resetConfirmationPanel.activeSelf;
    public bool IsBusyWithOtherWindow =>
        IsOfflineRewardVisible ||
        IsUpgradePanelVisible ||
        IsBuildPanelVisible ||
        IsMissionPanelVisible ||
        IsMenuVisible ||
        IsResetConfirmationVisible;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas == null)
        {
            rootCanvas = FindAnyObjectByType<Canvas>();
        }

        HideLegacyTextsIfBarsConfigured();
        HideSharedOverlay();
        HideMenu();
        HideUpgradePanel();
        HideBuildPanel();
        HideMissionPanel();
        HideResetConfirmation();
        HideOfflineReward();
        HideBoostOffer();
        SetMainScreenUpgradeButtonVisible(false);
        SetMainScreenBuildButtonVisible(false);
        SetMainScreenMissionButtonVisible(false);
        UpdateBoostUI(null);
    }

    private void Start()
    {
        ApplyResponsiveLayout(true);
    }

    private void Update()
    {
        ApplyResponsiveLayoutIfNeeded();

        if (lastDisplayedGameData == null)
        {
            return;
        }

        RefreshEnergyVisuals(lastDisplayedGameData, false);
    }

    public void UpdateUI(GameData gameData)
    {
        lastDisplayedGameData = gameData;

        if (oreText != null)
        {
            oreText.text = "Warehouse Ore: " + NumberFormatter.FormatInt(gameData.ore);
        }

        if (metalText != null)
        {
            metalText.text = "Metal: " + NumberFormatter.FormatInt(gameData.metal);
        }

        if (energyText != null)
        {
            bool shouldUseLegacyEnergyText = !IsEnergyBarActive();
            energyText.gameObject.SetActive(shouldUseLegacyEnergyText);
        }

        if (crystalText != null)
        {
            crystalText.text = "Crystal: " + NumberFormatter.FormatInt(gameData.crystal);
        }

        if (platformText != null)
        {
            bool shouldUseLegacyPlatformText = !IsPlatformBarActive();
            platformText.gameObject.SetActive(shouldUseLegacyPlatformText && gameData.hasMiningPlatform);

            if (shouldUseLegacyPlatformText && gameData.hasMiningPlatform)
            {
                platformText.text =
                    "Platform: " +
                    NumberFormatter.FormatInt(gameData.shuttleOre) +
                    " / " +
                    NumberFormatter.FormatInt(gameData.platformCapacity);
            }
        }

        bool shouldUseLegacyShuttleText = !IsShuttleBarActive();

        for (int shuttleIndex = 0; shuttleIndex < GameData.MaxShuttles; shuttleIndex++)
        {
            Text legacyShuttleText = GetLegacyShuttleText(shuttleIndex);

            if (legacyShuttleText == null)
            {
                continue;
            }

            bool isVisible = shouldUseLegacyShuttleText && shuttleIndex < gameData.ActiveShuttleCount;
            legacyShuttleText.gameObject.SetActive(isVisible);

            if (!isVisible)
            {
                continue;
            }

            legacyShuttleText.text =
                "Shuttle " +
                (shuttleIndex + 1) +
                ": " +
                NumberFormatter.FormatInt(GetDisplayedShuttleLoadForSlot(gameData, shuttleIndex)) +
                " / " +
                NumberFormatter.FormatInt(gameData.shuttleCapacity);
        }

        if (orePerSecondText != null)
        {
            orePerSecondText.text = "Ore / sec: " + NumberFormatter.FormatInt(gameData.orePerSecond);
        }

        if (orePerClickText != null)
        {
            orePerClickText.text = "Ore / click: " + NumberFormatter.FormatInt(gameData.orePerClick);
        }

        RefreshEnergyVisuals(gameData, !hasDisplayedEnergyAmount);

        UpdatePlatformBar(gameData);
        UpdateShuttleBars(gameData);

        for (int i = 0; i < GameData.MaxShuttles; i++)
        {
            UpdateSendButton(i, gameData);
        }

        if (produceMetalButton != null)
        {
            bool canShowProduceMetal = gameData.metalPerCraft > 0;
            produceMetalButton.gameObject.SetActive(canShowProduceMetal);
            produceMetalButton.interactable = canShowProduceMetal && CanProduceMetal(gameData);
        }

        if (produceMetalButtonText != null)
        {
            produceMetalButtonText.text = BuildMetalProductionText(gameData);
        }
    }

    public void InitializeUpgradeList(
        IReadOnlyList<UpgradeState> upgradeStates,
        Action<UpgradeState> onUpgradeBuyRequested)
    {
        ClearUpgradeList();

        if (upgradeListRoot == null || upgradeItemPrefab == null)
        {
            return;
        }

        if (upgradeStates != null)
        {
            for (int i = 0; i < upgradeStates.Count; i++)
            {
                UpgradeItemUI item = Instantiate(upgradeItemPrefab, upgradeListRoot);
                item.Initialize(upgradeStates[i], onUpgradeBuyRequested);
                upgradeItems.Add(item);
            }
        }

        RefreshUpgradeList(GetDisplayDataOrDefault());
    }

    public void InitializeShuttleButtons(Action<int> onShuttleSendRequested)
    {
        EnsureShuttleButtonSlot(0, sendShuttleButton, sendShuttleButtonText, onShuttleSendRequested);
        EnsureShuttleButtonSlot(1, sendShuttleButton2, sendShuttleButtonText2, onShuttleSendRequested);
        EnsureShuttleButtonSlot(2, sendShuttleButton3, sendShuttleButtonText3, onShuttleSendRequested);
        ApplyResponsiveLayout(true);
    }

    public void InitializeShuttleDisplays()
    {
        EnsureShuttleBarSlot(0, shuttleFillImage, shuttleBarText);
        EnsureShuttleBarSlot(1, shuttleFillImage2, shuttleBarText2);
        EnsureShuttleBarSlot(2, shuttleFillImage3, shuttleBarText3);
        ApplyResponsiveLayout(true);
    }

    public void InitializeMenuButtons(Action onExitRequested)
    {
        if (dropdownMenuPanel == null)
        {
            return;
        }

        Button button = exitMenuButton != null ? exitMenuButton : FindMenuButtonByName("ExitButton");

        if (button == null)
        {
            button = CreateRuntimeMenuButton("ExitButton");
        }

        if (button == null)
        {
            return;
        }

        Text buttonText = exitMenuButtonText != null
            ? exitMenuButtonText
            : button.GetComponentInChildren<Text>(true);

        exitMenuButton = button;
        exitMenuButtonText = buttonText;

        if (exitMenuButtonText != null)
        {
            exitMenuButtonText.text = "Exit";
        }

        exitMenuButton.onClick.RemoveAllListeners();
        exitMenuButton.onClick.AddListener(() => onExitRequested?.Invoke());
    }

    public void InitializeBoostOfferButton(Action<TemporaryBoostState> onAcceptRequested)
    {
        boostOfferAcceptHandler = onAcceptRequested;
        EnsureBoostOfferButtonSlot(0, boostOfferButton, boostOfferButtonText);
    }

    public void ShowBoostOffers(IReadOnlyList<TemporaryBoostState> boostStates)
    {
        if (boostStates == null || boostStates.Count <= 0)
        {
            HideBoostOffer();
            return;
        }

        for (int i = 0; i < boostStates.Count; i++)
        {
            EnsureBoostOfferButtonSlot(i, i == 0 ? boostOfferButton : null, i == 0 ? boostOfferButtonText : null);
            Button button = GetBoostOfferButton(i);
            Text buttonText = GetBoostOfferButtonText(i);

            if (button == null)
            {
                continue;
            }

            TemporaryBoostState boostState = boostStates[i];

            if (buttonText != null)
            {
                buttonText.text = BuildBoostOfferText(boostState);
            }

            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => boostOfferAcceptHandler?.Invoke(boostState));
            button.gameObject.SetActive(true);
        }

        for (int i = boostStates.Count; i < boostOfferButtons.Count; i++)
        {
            if (boostOfferButtons[i] != null)
            {
                boostOfferButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void RefreshBoostOfferTexts(IReadOnlyList<TemporaryBoostState> boostStates)
    {
        if (boostStates == null)
        {
            return;
        }

        for (int i = 0; i < boostStates.Count; i++)
        {
            Text buttonText = GetBoostOfferButtonText(i);

            if (buttonText != null)
            {
                buttonText.text = BuildBoostOfferText(boostStates[i]);
            }
        }
    }

    private void EnsureBoostOfferButtonSlot(int boostIndex, Button serializedButton, Text serializedText)
    {
        Button button = serializedButton;
        Text buttonText = serializedText;

        if (boostIndex >= 0 && boostIndex < boostOfferButtons.Count && boostOfferButtons[boostIndex] != null)
        {
            button = boostOfferButtons[boostIndex];
            buttonText = boostOfferButtonTexts[boostIndex];
        }

        if (boostOfferButton == null)
        {
            boostOfferButton = FindButtonByName("BoostOfferButton");
        }

        if (boostOfferButton == null)
        {
            boostOfferButton = FindButtonByName("Accept");
        }

        if (button == null)
        {
            if (boostIndex > 0)
            {
                button = FindButtonByName("BoostOfferButton" + (boostIndex + 1));
            }

            if (button == null)
            {
                button = boostIndex == 0 ? boostOfferButton : CreateRuntimeBoostOfferButton(boostIndex);
            }

            buttonText = button != null ? button.GetComponentInChildren<Text>(true) : null;
        }

        while (boostOfferButtons.Count <= boostIndex)
        {
            boostOfferButtons.Add(null);
            boostOfferButtonTexts.Add(null);
        }

        boostOfferButtons[boostIndex] = button;
        boostOfferButtonTexts[boostIndex] = buttonText;

        if (boostIndex == 0)
        {
            boostOfferButton = button;
            boostOfferButtonText = buttonText;
        }

        if (button == null)
        {
            return;
        }

        if (buttonText == null)
        {
            buttonText = button.GetComponentInChildren<Text>(true);
            boostOfferButtonTexts[boostIndex] = buttonText;

            if (boostIndex == 0)
            {
                boostOfferButtonText = buttonText;
            }
        }

        if (buttonText != null)
        {
            buttonText.text = "Accept";
        }

        button.onClick = new Button.ButtonClickedEvent();
    }

    public void RefreshUpgradeList(GameData gameData)
    {
        lastDisplayedGameData = gameData;

        for (int i = 0; i < upgradeItems.Count; i++)
        {
            upgradeItems[i].Refresh(gameData, selectedUpgradeCategory, upgradePanel != null && upgradePanel.activeSelf);
        }
    }

    public void InitializeBuildingList(
        IReadOnlyList<BuildingState> buildingStates,
        Action<BuildingState> onBuildingBuyRequested)
    {
        ClearBuildingList();

        if (buildListRoot == null || buildingItemPrefab == null)
        {
            return;
        }

        if (buildingStates != null)
        {
            for (int i = 0; i < buildingStates.Count; i++)
            {
                BuildingItemUI item = Instantiate(buildingItemPrefab, buildListRoot);
                item.Initialize(buildingStates[i], onBuildingBuyRequested);
                buildingItems.Add(item);
            }
        }

        RefreshBuildingList(GetDisplayDataOrDefault());
    }

    public void InitializeMetaBonusList(
        IReadOnlyList<MetaBonusState> metaBonusStates,
        Action<MetaBonusState> onMetaBonusBuyRequested)
    {
        ClearMetaBonusList();

        if (metaBonusListRoot == null || metaBonusItemPrefab == null)
        {
            return;
        }

        if (metaBonusStates != null)
        {
            for (int i = 0; i < metaBonusStates.Count; i++)
            {
                MetaBonusItemUI item = Instantiate(metaBonusItemPrefab, metaBonusListRoot);
                item.Initialize(metaBonusStates[i], onMetaBonusBuyRequested);
                metaBonusItems.Add(item);
            }
        }

        RefreshMetaBonusList(GetDisplayDataOrDefault());
    }

    public void RefreshBuildingList(GameData gameData)
    {
        lastDisplayedGameData = gameData;

        bool shouldShowBuildings = buildPanel != null && buildPanel.activeSelf;

        for (int i = 0; i < buildingItems.Count; i++)
        {
            buildingItems[i].Refresh(gameData, shouldShowBuildings);
        }
    }

    public void RefreshMetaBonusList(GameData gameData)
    {
        lastDisplayedGameData = gameData;

        bool shouldShowMetaBonuses = missionPanel != null && missionPanel.activeSelf;

        if (metaBonusListRoot != null)
        {
            metaBonusListRoot.gameObject.SetActive(shouldShowMetaBonuses);
        }

        for (int i = 0; i < metaBonusItems.Count; i++)
        {
            metaBonusItems[i].Refresh(gameData, shouldShowMetaBonuses);
        }
    }

    public void SetUpgradeCategory(UpgradeCategory category)
    {
        selectedUpgradeCategory = category;
        RefreshUpgradeList(GetDisplayDataOrDefault());
    }

    public void UpdateUpgradeCategoryTabs(
        bool minerUnlocked,
        bool platformUnlocked,
        bool powerUnlocked,
        bool factoryUnlocked,
        bool shuttleUnlocked)
    {
        SetTabInteractable(minerTabButton, minerUnlocked);
        SetTabInteractable(platformTabButton, platformUnlocked);
        SetTabInteractable(powerTabButton, powerUnlocked);
        SetTabInteractable(factoryTabButton, factoryUnlocked);
        SetTabInteractable(shuttleTabButton, shuttleUnlocked);

        if (!IsCategoryUnlocked(selectedUpgradeCategory, minerUnlocked, platformUnlocked, powerUnlocked, factoryUnlocked, shuttleUnlocked))
        {
            selectedUpgradeCategory = GetFirstUnlockedCategory(minerUnlocked, platformUnlocked, powerUnlocked, factoryUnlocked, shuttleUnlocked);
        }

        RefreshUpgradeList(GetDisplayDataOrDefault());
    }

    public void ToggleMenu()
    {
        if (dropdownMenuPanel == null)
        {
            return;
        }

        bool shouldShowMenu = !dropdownMenuPanel.activeSelf;
        dropdownMenuPanel.SetActive(shouldShowMenu);
        SetSharedOverlayVisible(shouldShowMenu);

        if (!shouldShowMenu)
        {
            HideResetConfirmation();
        }
    }

    public void HideMenu()
    {
        if (dropdownMenuPanel != null)
        {
            dropdownMenuPanel.SetActive(false);
        }

        HideSharedOverlay();
    }

    public void OpenUpgradePanel()
    {
        if (upgradePanel == null)
        {
            return;
        }

        bool wasVisible = upgradePanel.activeSelf;

        if (wasVisible)
        {
            upgradePanel.SetActive(false);
            RefreshPanelLists();
            return;
        }

        HideBuildPanel();
        HideMissionPanel();
        HideBoostOffer();
        upgradePanel.SetActive(true);
        RefreshPanelLists();
    }

    public void OpenBuildingPanel()
    {
        if (buildPanel == null)
        {
            return;
        }

        bool wasVisible = buildPanel.activeSelf;

        if (wasVisible)
        {
            buildPanel.SetActive(false);
            RefreshPanelLists();
            return;
        }

        HideUpgradePanel();
        HideMissionPanel();
        HideBoostOffer();
        buildPanel.SetActive(true);
        RefreshPanelLists();
    }

    public void OpenMissionPanel()
    {
        if (missionPanel == null)
        {
            return;
        }

        bool wasVisible = missionPanel.activeSelf;

        if (wasVisible)
        {
            missionPanel.SetActive(false);
            RefreshPanelLists();
            return;
        }

        HideUpgradePanel();
        HideBuildPanel();
        HideBoostOffer();
        missionPanel.SetActive(true);
        RefreshPanelLists();
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        RefreshPanelLists();
    }

    public void HideBuildPanel()
    {
        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }

        RefreshPanelLists();
    }

    public void HideMissionPanel()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }

        RefreshPanelLists();
    }

    public void SetMainScreenUpgradeButtonVisible(bool isVisible)
    {
        if (mainScreenUpgradeButton != null)
        {
            mainScreenUpgradeButton.SetActive(isVisible);
        }
    }

    public void SetMainScreenBuildButtonVisible(bool isVisible)
    {
        if (mainScreenBuildButton != null)
        {
            mainScreenBuildButton.SetActive(isVisible);
        }
    }

    public void SetMainScreenMissionButtonVisible(bool isVisible)
    {
        if (mainScreenMissionButton != null)
        {
            mainScreenMissionButton.SetActive(isVisible);
        }
    }

    public void UpdateMissionInfo(MissionProgressData progressData, string noMissionsText)
    {
        bool hasMission = progressData.hasMission;

        if (metaBonusHeaderText != null)
        {
            metaBonusHeaderText.gameObject.SetActive(true);
            metaBonusHeaderText.text = "Meta Bonuses";
        }

        if (metaBonusListRoot != null)
        {
            metaBonusListRoot.gameObject.SetActive(true);
        }

        if (missionStatusText != null)
        {
            missionStatusText.gameObject.SetActive(true);

            if (!hasMission)
            {
                missionStatusText.text = string.IsNullOrWhiteSpace(noMissionsText)
                    ? ShuttleConfig.DefaultNoMissionsText
                    : noMissionsText;
            }
            else
            {
                missionStatusText.text =
                    "Mission " +
                    progressData.missionNumber +
                    "/" +
                    progressData.missionCount +
                    (progressData.canClaimReward ? " Complete" : string.Empty);
            }
        }

        if (missionTitleText != null)
        {
            missionTitleText.gameObject.SetActive(hasMission);
            missionTitleText.text = hasMission ? progressData.missionName : string.Empty;
        }

        if (missionDescriptionText != null)
        {
            missionDescriptionText.gameObject.SetActive(hasMission);
            missionDescriptionText.text = hasMission ? progressData.description : string.Empty;
        }

        if (missionProgressText != null)
        {
            missionProgressText.gameObject.SetActive(hasMission);

            if (!hasMission)
            {
                missionProgressText.text = string.Empty;
            }
            else if (progressData.isCompleted)
            {
                missionProgressText.text =
                    progressData.progressLabel + ": " +
                    NumberFormatter.FormatInt(progressData.targetValue) + " / " +
                    NumberFormatter.FormatInt(progressData.targetValue);
            }
            else
            {
                missionProgressText.text =
                    progressData.progressLabel + ": " +
                    NumberFormatter.FormatInt(progressData.currentValue) + " / " +
                    NumberFormatter.FormatInt(progressData.targetValue);
            }
        }

        if (missionRewardText != null)
        {
            missionRewardText.gameObject.SetActive(hasMission);

            if (!hasMission)
            {
                missionRewardText.text = string.Empty;
            }
            else
            {
                missionRewardText.text =
                    (progressData.canClaimReward ? "Reward ready: " : "Reward: ") +
                    NumberFormatter.FormatInt(progressData.crystalReward) +
                    " Crystal";
            }
        }

        if (missionClaimButton != null)
        {
            missionClaimButton.gameObject.SetActive(progressData.canClaimReward);
            missionClaimButton.interactable = progressData.canClaimReward;
        }

        if (missionClaimButtonText != null)
        {
            missionClaimButtonText.text = "Claim Reward";
        }
    }

    public void ShowResetConfirmation()
    {
        HideAllContentPanels();

        if (resetConfirmationPanel != null)
        {
            resetConfirmationPanel.SetActive(true);
        }
    }

    public void HideResetConfirmation()
    {
        if (resetConfirmationPanel != null)
        {
            resetConfirmationPanel.SetActive(false);
        }
    }

    public void ShowOfflineReward(int warehouseAmount)
    {
        HideMenu();
        HideAllContentPanels();
        HideResetConfirmation();

        if (offlineRewardText != null)
        {
            offlineRewardText.text = "Sent to warehouse while offline: " +
                                     NumberFormatter.FormatInt(warehouseAmount) +
                                     " Ore.";
        }

        if (offlineRewardPanel != null)
        {
            offlineRewardPanel.SetActive(true);
        }
    }

    public void HideOfflineReward()
    {
        if (offlineRewardPanel != null)
        {
            offlineRewardPanel.SetActive(false);
        }
    }

    public void ShowBoostOffer(TemporaryBoostState boostState)
    {
        if (boostState == null)
        {
            HideBoostOffer();
            return;
        }

        ShowBoostOffers(new[] { boostState });
    }

    public void HideBoostOffer()
    {
        for (int i = 0; i < boostOfferButtons.Count; i++)
        {
            if (boostOfferButtons[i] != null)
            {
                boostOfferButtons[i].gameObject.SetActive(false);
            }
        }

        if (boostOfferButtons.Count <= 0 && boostOfferButton != null)
        {
            boostOfferButton.gameObject.SetActive(false);
        }
    }

    public void UpdateBoostUI(IReadOnlyList<TemporaryBoostState> activeBoostStates)
    {
        if (boostStatusText == null)
        {
            return;
        }

        bool hasActiveBoost = activeBoostStates != null && activeBoostStates.Count > 0;
        boostStatusText.gameObject.SetActive(hasActiveBoost);

        if (!hasActiveBoost)
        {
            boostStatusText.text = string.Empty;
            return;
        }

        List<string> lines = new List<string>();

        for (int i = 0; i < activeBoostStates.Count; i++)
        {
            TemporaryBoostState boostState = activeBoostStates[i];
            lines.Add(
                boostState.Definition.boostName +
                " x" + NumberFormatter.FormatFloat(boostState.GetMultiplier()) +
                " - " + Mathf.CeilToInt(boostState.ActiveRemainingTime) + "s");
        }

        boostStatusText.text = string.Join("\n", lines);
    }

    private void SetSharedOverlayVisible(bool isVisible)
    {
        if (sharedOverlayPanel != null)
        {
            sharedOverlayPanel.SetActive(isVisible);
        }
    }

    private void HideSharedOverlay()
    {
        SetSharedOverlayVisible(false);
    }

    private void HideLegacyTextsIfBarsConfigured()
    {
        if (energyText != null)
        {
            energyText.gameObject.SetActive(!IsEnergyBarActive());
        }

        if (platformText != null)
        {
            platformText.gameObject.SetActive(!IsPlatformBarActive());
        }

        SetLegacyShuttleTextsVisible(!IsShuttleBarActive());
    }

    private void ClearUpgradeList()
    {
        for (int i = 0; i < upgradeItems.Count; i++)
        {
            if (upgradeItems[i] != null)
            {
                Destroy(upgradeItems[i].gameObject);
            }
        }

        upgradeItems.Clear();
    }

    private void ClearBuildingList()
    {
        for (int i = 0; i < buildingItems.Count; i++)
        {
            if (buildingItems[i] != null)
            {
                Destroy(buildingItems[i].gameObject);
            }
        }

        buildingItems.Clear();
    }

    private void ClearMetaBonusList()
    {
        for (int i = 0; i < metaBonusItems.Count; i++)
        {
            if (metaBonusItems[i] != null)
            {
                Destroy(metaBonusItems[i].gameObject);
            }
        }

        metaBonusItems.Clear();
    }

    private void RefreshPanelLists()
    {
        GameData displayData = GetDisplayDataOrDefault();
        RefreshUpgradeList(displayData);
        RefreshBuildingList(displayData);
        RefreshMetaBonusList(displayData);
    }

    private GameData GetDisplayDataOrDefault()
    {
        return lastDisplayedGameData ?? new GameData();
    }

    private void HideAllContentPanels()
    {
        HideUpgradePanel();
        HideBuildPanel();
        HideMissionPanel();
    }

    private void SetTabInteractable(Button button, bool isInteractable)
    {
        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }

    private bool IsCategoryUnlocked(
        UpgradeCategory category,
        bool minerUnlocked,
        bool platformUnlocked,
        bool powerUnlocked,
        bool factoryUnlocked,
        bool shuttleUnlocked)
    {
        switch (category)
        {
            case UpgradeCategory.Miner:
                return minerUnlocked;

            case UpgradeCategory.Platform:
                return platformUnlocked;

            case UpgradeCategory.PowerStation:
                return powerUnlocked;

            case UpgradeCategory.Factory:
                return factoryUnlocked;

            case UpgradeCategory.Shuttle:
                return shuttleUnlocked;

            default:
                return false;
        }
    }

    private UpgradeCategory GetFirstUnlockedCategory(
        bool minerUnlocked,
        bool platformUnlocked,
        bool powerUnlocked,
        bool factoryUnlocked,
        bool shuttleUnlocked)
    {
        if (minerUnlocked)
        {
            return UpgradeCategory.Miner;
        }

        if (platformUnlocked)
        {
            return UpgradeCategory.Platform;
        }

        if (powerUnlocked)
        {
            return UpgradeCategory.PowerStation;
        }

        if (factoryUnlocked)
        {
            return UpgradeCategory.Factory;
        }

        if (shuttleUnlocked)
        {
            return UpgradeCategory.Shuttle;
        }

        return UpgradeCategory.Miner;
    }

    private string BuildBoostOfferText(TemporaryBoostState boostState)
    {
        string targetText;

        switch (boostState.Definition.targetType)
        {
            case TemporaryBoostTargetType.OrePerClick:
                targetText = "Ore / click";
                break;

            case TemporaryBoostTargetType.ShuttleTravelSpeed:
                targetText = "Shuttle speed";
                break;

            default:
                targetText = "Ore / sec";
                break;
        }

        string description = string.IsNullOrWhiteSpace(boostState.Definition.description)
            ? string.Empty
            : boostState.Definition.description + "\n";

        return boostState.Definition.boostName + "\n" +
               description +
               "x" +
               NumberFormatter.FormatFloat(boostState.GetMultiplier()) +
               " " +
               targetText +
               " for " +
               Mathf.RoundToInt(boostState.Definition.durationSeconds) +
               "s";
    }

    private bool CanProduceMetal(GameData gameData)
    {
        return gameData != null &&
               gameData.ore >= gameData.metalOreCost &&
               gameData.energy >= gameData.metalEnergyCost;
    }

    private bool IsEnergyBarActive()
    {
        return IsBarActive(energyFillImage, energyBarText);
    }

    private bool IsPlatformBarActive()
    {
        return IsBarActive(platformFillImage, platformBarText);
    }

    private bool IsShuttleBarActive()
    {
        if (IsBarActive(shuttleFillImage, shuttleBarText) ||
            IsBarActive(shuttleFillImage2, shuttleBarText2) ||
            IsBarActive(shuttleFillImage3, shuttleBarText3))
        {
            return true;
        }

        int slotCount = Math.Max(shuttleFillImages.Count, shuttleBarTexts.Count);

        for (int i = 0; i < slotCount; i++)
        {
            Image fillImage = i < shuttleFillImages.Count ? shuttleFillImages[i] : null;
            Text barText = i < shuttleBarTexts.Count ? shuttleBarTexts[i] : null;

            if (IsBarActive(fillImage, barText))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBarActive(Graphic fillGraphic, Graphic textGraphic)
    {
        return IsGraphicActive(fillGraphic) || IsGraphicActive(textGraphic);
    }

    private void SetLegacyShuttleTextsVisible(bool isVisible)
    {
        Text legacyShuttleText1 = GetLegacyShuttleText(0);
        Text legacyShuttleText2 = GetLegacyShuttleText(1);
        Text legacyShuttleText3 = GetLegacyShuttleText(2);

        if (legacyShuttleText1 != null)
        {
            legacyShuttleText1.gameObject.SetActive(isVisible);
        }

        if (legacyShuttleText2 != null)
        {
            legacyShuttleText2.gameObject.SetActive(isVisible);
        }

        if (legacyShuttleText3 != null)
        {
            legacyShuttleText3.gameObject.SetActive(isVisible);
        }
    }

    private Text GetLegacyShuttleText(int shuttleIndex)
    {
        switch (shuttleIndex)
        {
            case 0:
                return shuttleText;
            case 1:
                return shuttleText2;
            case 2:
                return shuttleText3;
            default:
                return null;
        }
    }

    private bool IsGraphicActive(Graphic graphic)
    {
        return graphic != null && graphic.gameObject.activeInHierarchy;
    }

    private void ApplyResponsiveLayoutIfNeeded()
    {
        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();

            if (rootCanvas == null)
            {
                rootCanvas = FindAnyObjectByType<Canvas>();
            }
        }

        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);

        if (currentScreenSize == lastAppliedScreenSize &&
            Screen.safeArea == lastAppliedSafeArea)
        {
            return;
        }

        ApplyResponsiveLayout(false);
    }

    private void ApplyResponsiveLayout(bool force)
    {
        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();

            if (rootCanvas == null)
            {
                rootCanvas = FindAnyObjectByType<Canvas>();
            }
        }

        RectTransform canvasRect = rootCanvas != null
            ? rootCanvas.GetComponent<RectTransform>()
            : null;

        if (canvasRect == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;
        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);

        if (!force &&
            currentScreenSize == lastAppliedScreenSize &&
            safeArea == lastAppliedSafeArea)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        float widthScale = canvasRect.rect.width / Screen.width;
        float heightScale = canvasRect.rect.height / Screen.height;
        float topInset = ((Screen.height - safeArea.yMax) * heightScale) + safeAreaExtraTopPadding;
        float leftInset = (safeArea.xMin * widthScale) + safeAreaExtraSidePadding;
        float rightInset = ((Screen.width - safeArea.xMax) * widthScale) + safeAreaExtraSidePadding;
        float bottomInset = safeArea.yMin * heightScale;

        ApplyTopSafeArea(topInset, leftInset, rightInset);
        ApplyStretchPanelSafeArea(upgradePanel, leftInset, rightInset, topInset, bottomInset);
        ApplyStretchPanelSafeArea(buildPanel, leftInset, rightInset, topInset, bottomInset);
        ApplyStretchPanelSafeArea(missionPanel, leftInset, rightInset, topInset, bottomInset);
        ApplyCenteredPopupSafeArea(resetConfirmationPanel, topInset);
        ApplyCenteredPopupSafeArea(offlineRewardPanel, topInset);
        ApplyLayoutPadding(upgradeListRoot, upgradeContentTopPadding);
        ApplyLayoutPadding(buildListRoot, buildContentTopPadding);

        lastAppliedSafeArea = safeArea;
        lastAppliedScreenSize = currentScreenSize;
    }

    private void ApplyTopSafeArea(float topInset, float leftInset, float rightInset)
    {
        RectTransform energyBarRoot = GetBarRoot(energyFillImage, energyBarText);
        RectTransform platformBarRoot = GetBarRoot(platformFillImage, platformBarText);

        ApplyTopAnchoredOffset(GetRectTransform(oreText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(orePerSecondText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(orePerClickText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(energyText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(metalText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(crystalText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(platformText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(shuttleText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(shuttleText2), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(shuttleText3), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(boostStatusText), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(energyBarRoot, topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(platformBarRoot, topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetShuttleBarRoot(0), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetShuttleBarRoot(1), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetShuttleBarRoot(2), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(produceMetalButton), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(sendShuttleButton), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(sendShuttleButton2), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetRectTransform(sendShuttleButton3), topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(dropdownMenuPanel != null ? dropdownMenuPanel.GetComponent<RectTransform>() : null, topInset, leftInset, rightInset);
        ApplyTopAnchoredOffset(GetMenuButtonRect(), topInset, leftInset, rightInset);
    }

    private void ApplyTopAnchoredOffset(RectTransform rectTransform, float topInset, float leftInset, float rightInset)
    {
        if (rectTransform == null)
        {
            return;
        }

        CacheAnchoredPosition(rectTransform);

        Vector2 basePosition = baseAnchoredPositions[rectTransform];
        Vector2 adjustedPosition = basePosition;

        if (rectTransform.anchorMin.y >= 0.5f && rectTransform.anchorMax.y >= 0.5f)
        {
            adjustedPosition.y = basePosition.y - topInset;
        }

        if (rectTransform.anchorMin.x >= 0.99f && rectTransform.anchorMax.x >= 0.99f)
        {
            adjustedPosition.x = basePosition.x - rightInset;
        }
        else if (rectTransform.anchorMin.x <= 0.01f && rectTransform.anchorMax.x <= 0.01f)
        {
            adjustedPosition.x = basePosition.x + leftInset;
        }

        rectTransform.anchoredPosition = adjustedPosition;
    }

    private void ApplyStretchPanelSafeArea(GameObject panelObject, float leftInset, float rightInset, float topInset, float bottomInset)
    {
        if (panelObject == null)
        {
            return;
        }

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            return;
        }

        CacheStretchOffsets(rectTransform);
        rectTransform.offsetMin = baseOffsetMins[rectTransform] + new Vector2(leftInset, bottomInset);
        rectTransform.offsetMax = baseOffsetMaxes[rectTransform] + new Vector2(-rightInset, -topInset);
    }

    private void ApplyCenteredPopupSafeArea(GameObject popupObject, float topInset)
    {
        if (popupObject == null)
        {
            return;
        }

        RectTransform rectTransform = popupObject.GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            return;
        }

        CacheAnchoredPosition(rectTransform);
        Vector2 basePosition = baseAnchoredPositions[rectTransform];
        rectTransform.anchoredPosition = new Vector2(
            basePosition.x,
            basePosition.y - (topInset * centeredPopupYOffsetFactor));
    }

    private void ApplyLayoutPadding(Transform listRoot, int extraTopPadding)
    {
        if (listRoot == null)
        {
            return;
        }

        VerticalLayoutGroup layoutGroup = listRoot.GetComponent<VerticalLayoutGroup>();

        if (layoutGroup == null)
        {
            return;
        }

        CacheLayoutPadding(layoutGroup);
        RectOffset basePadding = baseLayoutPaddings[layoutGroup];
        layoutGroup.padding = new RectOffset(
            basePadding.left,
            basePadding.right,
            basePadding.top + Math.Max(0, extraTopPadding),
            basePadding.bottom);
    }

    private RectTransform GetRectTransform(Component component)
    {
        return component != null ? component.GetComponent<RectTransform>() : null;
    }

    private RectTransform GetBarRoot(Graphic fillGraphic, Graphic textGraphic)
    {
        RectTransform fillRect = GetRectTransform(fillGraphic);

        if (fillRect != null && fillRect.parent is RectTransform fillParent)
        {
            return fillParent;
        }

        RectTransform textRect = GetRectTransform(textGraphic);

        if (textRect != null && textRect.parent is RectTransform textParent)
        {
            return textParent;
        }

        return null;
    }

    private RectTransform GetMenuButtonRect()
    {
        if (cachedMenuButtonRect != null)
        {
            return cachedMenuButtonRect;
        }

        cachedMenuButtonRect = FindRectTransformByName("MenuButton");
        return cachedMenuButtonRect;
    }

    private RectTransform FindRectTransformByName(string objectName)
    {
        if (rootCanvas == null)
        {
            return null;
        }

        Transform[] transforms = rootCanvas.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == objectName)
            {
                return transforms[i] as RectTransform;
            }
        }

        return null;
    }

    private Button FindButtonByName(string objectName)
    {
        RectTransform rectTransform = FindRectTransformByName(objectName);
        return rectTransform != null ? rectTransform.GetComponent<Button>() : null;
    }

    private void CacheAnchoredPosition(RectTransform rectTransform)
    {
        if (rectTransform != null && !baseAnchoredPositions.ContainsKey(rectTransform))
        {
            baseAnchoredPositions.Add(rectTransform, rectTransform.anchoredPosition);
        }
    }

    private void CacheStretchOffsets(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return;
        }

        if (!baseOffsetMins.ContainsKey(rectTransform))
        {
            baseOffsetMins.Add(rectTransform, rectTransform.offsetMin);
        }

        if (!baseOffsetMaxes.ContainsKey(rectTransform))
        {
            baseOffsetMaxes.Add(rectTransform, rectTransform.offsetMax);
        }
    }

    private void CacheLayoutPadding(VerticalLayoutGroup layoutGroup)
    {
        if (layoutGroup == null || baseLayoutPaddings.ContainsKey(layoutGroup))
        {
            return;
        }

        RectOffset padding = layoutGroup.padding;
        baseLayoutPaddings.Add(
            layoutGroup,
            new RectOffset(padding.left, padding.right, padding.top, padding.bottom));
    }

    private void RefreshEnergyVisuals(GameData gameData, bool snapToTarget)
    {
        if (gameData == null)
        {
            return;
        }

        float targetDisplayedEnergy = GetEnergyDisplayTarget(gameData);

        if (!hasDisplayedEnergyAmount || snapToTarget)
        {
            displayedEnergyAmount = targetDisplayedEnergy;
            hasDisplayedEnergyAmount = true;
        }
        else
        {
            float maxDelta = Mathf.Max(0.1f, energyDisplaySpeed) * Time.unscaledDeltaTime * Mathf.Max(1f, gameData.energyMax);
            displayedEnergyAmount = Mathf.MoveTowards(displayedEnergyAmount, targetDisplayedEnergy, maxDelta);
        }

        displayedEnergyAmount = Mathf.Clamp(displayedEnergyAmount, 0f, Mathf.Max(0, gameData.energyMax));

        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = gameData.energyMax > 0
                ? Mathf.Clamp01(displayedEnergyAmount / gameData.energyMax)
                : 0f;
        }

        string displayedEnergyText = FormatEnergyValue(displayedEnergyAmount);
        string energyMaxText = NumberFormatter.FormatInt(gameData.energyMax);

        if (energyBarText != null)
        {
            energyBarText.text = displayedEnergyText + " / " + energyMaxText;
        }

        if (energyText != null && energyText.gameObject.activeSelf)
        {
            energyText.text = "Energy: " + displayedEnergyText + " / " + energyMaxText;
        }
    }

    private float GetEnergyDisplayTarget(GameData gameData)
    {
        if (gameData == null || gameData.energyMax <= 0)
        {
            return 0f;
        }

        float targetDisplayedEnergy = gameData.energy;

        if (gameData.energy < gameData.energyMax &&
            gameData.energyRegenAmount > 0 &&
            gameData.energyRegenInterval > 0f)
        {
            float regenProgress = Mathf.Clamp01(gameData.energyRegenTimer / gameData.energyRegenInterval);
            float pendingEnergy = Mathf.Min(
                gameData.energyRegenAmount,
                gameData.energyMax - gameData.energy);
            targetDisplayedEnergy += pendingEnergy * regenProgress;
        }

        return Mathf.Clamp(targetDisplayedEnergy, 0f, gameData.energyMax);
    }

    private string FormatEnergyValue(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value))
            ? NumberFormatter.FormatInt(Mathf.RoundToInt(value))
            : value.ToString("0.0");
    }

    private void UpdatePlatformBar(GameData gameData)
    {
        if (gameData == null)
        {
            return;
        }

        int currentPlatformOre = gameData.hasMiningPlatform ? gameData.shuttleOre : 0;
        int maxPlatformOre = gameData.hasMiningPlatform ? gameData.platformCapacity : 0;

        if (platformFillImage != null)
        {
            platformFillImage.fillAmount = maxPlatformOre > 0
                ? Mathf.Clamp01(currentPlatformOre / (float)maxPlatformOre)
                : 0f;
        }

        if (platformBarText != null)
        {
            platformBarText.text =
                NumberFormatter.FormatInt(currentPlatformOre) +
                " / " +
                NumberFormatter.FormatInt(maxPlatformOre);
        }
    }

    private void UpdateShuttleBars(GameData gameData)
    {
        if (gameData == null)
        {
            return;
        }

        for (int shuttleIndex = 0; shuttleIndex < GameData.MaxShuttles; shuttleIndex++)
        {
            RectTransform barRoot = GetShuttleBarRoot(shuttleIndex);
            Image fillImage = GetShuttleFillImage(shuttleIndex);
            Text barText = GetShuttleBarText(shuttleIndex);

            if (barRoot == null && fillImage == null && barText == null)
            {
                continue;
            }

            bool isVisible = shuttleIndex < gameData.ActiveShuttleCount;

            if (barRoot != null && shuttleIndex > 0)
            {
                barRoot.gameObject.SetActive(isVisible);
            }
            else
            {
                if (fillImage != null && shuttleIndex > 0)
                {
                    fillImage.gameObject.SetActive(isVisible);
                }

                if (barText != null && shuttleIndex > 0)
                {
                    barText.gameObject.SetActive(isVisible);
                }
            }

            if (!isVisible)
            {
                continue;
            }

            int currentShuttleOre = GetDisplayedShuttleLoadForSlot(gameData, shuttleIndex);
            int maxShuttleOre = gameData.shuttleCapacity;

            if (fillImage != null)
            {
                fillImage.fillAmount = maxShuttleOre > 0
                    ? Mathf.Clamp01(currentShuttleOre / (float)maxShuttleOre)
                    : 0f;
            }

            if (barText != null)
            {
                barText.text =
                    NumberFormatter.FormatInt(currentShuttleOre) +
                    " / " +
                    NumberFormatter.FormatInt(maxShuttleOre);
            }
        }
    }

    private int GetDisplayedShuttleLoad(GameData gameData)
    {
        if (gameData == null)
        {
            return 0;
        }

        if (!gameData.hasMiningPlatform)
        {
            if (gameData.shuttleLoadingCooldownRemaining > 0f || gameData.shuttleLoadingOre > 0)
            {
                return Mathf.Max(0, gameData.shuttleLoadingOre);
            }

            if (gameData.shuttleDeliveringOre > 0 || gameData.shuttleSendCooldownRemaining > 0f)
            {
                return Mathf.Max(0, gameData.shuttleDeliveringOre);
            }

            return Mathf.Max(0, gameData.shuttleOre);
        }

        int totalShuttleOre = 0;

        for (int i = 0; i < gameData.ActiveShuttleCount; i++)
        {
            ShuttleState shuttleState = gameData.GetShuttleState(i);
            int shuttleLoad;

            if (shuttleState.loadingCooldownRemaining > 0f || shuttleState.loadingOre > 0)
            {
                shuttleLoad = Mathf.Clamp(shuttleState.dockedOre + shuttleState.loadingOre, 0, gameData.shuttleCapacity);
            }
            else if (shuttleState.dockedOre > 0)
            {
                shuttleLoad = Mathf.Max(0, shuttleState.dockedOre);
            }
            else
            {
                shuttleLoad = Mathf.Max(0, shuttleState.deliveringOre);
            }

            totalShuttleOre += shuttleLoad;
        }

        return totalShuttleOre;
    }

    private void UpdateSendButton(int shuttleIndex, GameData gameData)
    {
        Button button = GetSendButton(shuttleIndex);
        Text buttonText = GetSendButtonText(shuttleIndex);

        if (button == null && buttonText == null)
        {
            return;
        }

        bool isVisible = shuttleIndex < gameData.ActiveShuttleCount;

        if (button != null)
        {
            button.gameObject.SetActive(isVisible);
        }
        else if (buttonText != null)
        {
            buttonText.gameObject.SetActive(isVisible);
        }

        if (!isVisible)
        {
            return;
        }

        ShuttleState shuttleState = gameData.GetShuttleState(shuttleIndex);
        bool shuttleIsIdle =
            shuttleState.loadingTargetOre <= 0 &&
            shuttleState.loadingOre <= 0 &&
            shuttleState.deliveringOre <= 0 &&
            shuttleState.loadingCooldownRemaining <= 0f &&
            shuttleState.sendCooldownRemaining <= 0f;
        int currentLoad = GetDisplayedShuttleLoadForSlot(gameData, shuttleIndex);

        if (button != null)
        {
            button.interactable = gameData.hasMiningPlatform
                ? shuttleIsIdle && (shuttleState.dockedOre > 0 || gameData.shuttleOre > 0)
                : shuttleIndex == 0 && shuttleIsIdle && gameData.shuttleOre > 0;
        }

        if (buttonText == null)
        {
            return;
        }

        if (shuttleState.loadingCooldownRemaining > 0f)
        {
            buttonText.text = "Loading " + (shuttleIndex + 1) + " " + FormatTimer(shuttleState.loadingCooldownRemaining);
            return;
        }

        if (shuttleState.sendCooldownRemaining > 0f)
        {
            buttonText.text = "Flying " + (shuttleIndex + 1) + " " + FormatTimer(shuttleState.sendCooldownRemaining);
            return;
        }

        buttonText.text =
            "Send " +
            (shuttleIndex + 1) +
            " " +
            NumberFormatter.FormatInt(currentLoad) +
            "/" +
            NumberFormatter.FormatInt(gameData.shuttleCapacity);
    }

    private int GetDisplayedShuttleLoadForSlot(GameData gameData, int shuttleIndex)
    {
        if (gameData == null)
        {
            return 0;
        }

        if (!gameData.hasMiningPlatform)
        {
            return shuttleIndex == 0 ? Mathf.Max(0, gameData.shuttleOre) : 0;
        }

        ShuttleState shuttleState = gameData.GetShuttleState(shuttleIndex);

        if (shuttleState.loadingCooldownRemaining > 0f || shuttleState.loadingOre > 0)
        {
            return Mathf.Clamp(shuttleState.dockedOre + shuttleState.loadingOre, 0, gameData.shuttleCapacity);
        }

        if (shuttleState.dockedOre > 0)
        {
            return Mathf.Max(0, shuttleState.dockedOre);
        }

        return Mathf.Max(0, shuttleState.deliveringOre);
    }

    private Button GetSendButton(int shuttleIndex)
    {
        if (shuttleIndex >= 0 && shuttleIndex < shuttleButtons.Count && shuttleButtons[shuttleIndex] != null)
        {
            return shuttleButtons[shuttleIndex];
        }

        switch (shuttleIndex)
        {
            case 0:
                return sendShuttleButton;
            case 1:
                return sendShuttleButton2;
            case 2:
                return sendShuttleButton3;
            default:
                return null;
        }
    }

    private Text GetSendButtonText(int shuttleIndex)
    {
        if (shuttleIndex >= 0 && shuttleIndex < shuttleButtonTexts.Count && shuttleButtonTexts[shuttleIndex] != null)
        {
            return shuttleButtonTexts[shuttleIndex];
        }

        switch (shuttleIndex)
        {
            case 0:
                return sendShuttleButtonText;
            case 1:
                return sendShuttleButtonText2;
            case 2:
                return sendShuttleButtonText3;
            default:
                return null;
        }
    }

    private void EnsureShuttleButtonSlot(
        int shuttleIndex,
        Button serializedButton,
        Text serializedText,
        Action<int> onShuttleSendRequested)
    {
        Button button = serializedButton;
        Text buttonText = serializedText;

        if (button == null && sendShuttleButton != null)
        {
            Button clonedButton = CreateRuntimeShuttleButton(shuttleIndex);
            button = clonedButton;
            buttonText = clonedButton != null ? clonedButton.GetComponentInChildren<Text>(true) : null;
        }

        RegisterShuttleButtonSlot(shuttleIndex, button, buttonText, onShuttleSendRequested);
    }

    private void EnsureShuttleBarSlot(
        int shuttleIndex,
        Image serializedFillImage,
        Text serializedBarText)
    {
        Image fillImage = serializedFillImage;
        Text barText = serializedBarText;
        RectTransform barRoot = GetBarRoot(fillImage, barText);

        if (barRoot == null && shuttleFillImage != null && shuttleIndex > 0)
        {
            RectTransform clonedBarRoot = CreateRuntimeShuttleBar(shuttleIndex);
            fillImage = FindBarFillImage(clonedBarRoot);
            barText = clonedBarRoot != null ? clonedBarRoot.GetComponentInChildren<Text>(true) : null;
            barRoot = clonedBarRoot;
        }

        RegisterShuttleBarSlot(shuttleIndex, barRoot, fillImage, barText);
    }

    private Button CreateRuntimeShuttleButton(int shuttleIndex)
    {
        if (sendShuttleButton == null)
        {
            return null;
        }

        GameObject clonedObject = Instantiate(sendShuttleButton.gameObject, sendShuttleButton.transform.parent);
        clonedObject.name = "SendShuttleButton" + (shuttleIndex + 1);

        RectTransform templateRect = sendShuttleButton.GetComponent<RectTransform>();
        RectTransform clonedRect = clonedObject.GetComponent<RectTransform>();

        if (templateRect != null && clonedRect != null)
        {
            clonedRect.anchorMin = templateRect.anchorMin;
            clonedRect.anchorMax = templateRect.anchorMax;
            clonedRect.pivot = templateRect.pivot;
            clonedRect.sizeDelta = templateRect.sizeDelta;
            clonedRect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0f, -((shuttleIndex) * (templateRect.sizeDelta.y + 12f)));
        }

        return clonedObject.GetComponent<Button>();
    }

    private Button CreateRuntimeBoostOfferButton(int boostIndex)
    {
        if (boostOfferButton == null)
        {
            return null;
        }

        GameObject clonedObject = Instantiate(boostOfferButton.gameObject, boostOfferButton.transform.parent);
        clonedObject.name = "BoostOfferButton" + (boostIndex + 1);

        RectTransform templateRect = boostOfferButton.GetComponent<RectTransform>();
        RectTransform clonedRect = clonedObject.GetComponent<RectTransform>();

        if (templateRect != null && clonedRect != null)
        {
            clonedRect.anchorMin = templateRect.anchorMin;
            clonedRect.anchorMax = templateRect.anchorMax;
            clonedRect.pivot = templateRect.pivot;
            clonedRect.sizeDelta = templateRect.sizeDelta;
            clonedRect.localScale = templateRect.localScale;
            clonedRect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0f, -((boostIndex) * (templateRect.sizeDelta.y + 12f)));
        }

        return clonedObject.GetComponent<Button>();
    }

    private RectTransform CreateRuntimeShuttleBar(int shuttleIndex)
    {
        RectTransform templateRoot = GetBarRoot(shuttleFillImage, shuttleBarText);

        if (templateRoot == null)
        {
            return null;
        }

        GameObject clonedObject = Instantiate(templateRoot.gameObject, templateRoot.parent);
        clonedObject.name = "ShuttleBarRoot" + (shuttleIndex + 1);

        RectTransform clonedRect = clonedObject.GetComponent<RectTransform>();

        if (clonedRect == null)
        {
            return null;
        }

        clonedRect.anchorMin = templateRoot.anchorMin;
        clonedRect.anchorMax = templateRoot.anchorMax;
        clonedRect.pivot = templateRoot.pivot;
        clonedRect.sizeDelta = templateRoot.sizeDelta;
        clonedRect.localScale = templateRoot.localScale;

        RectTransform templateButtonRect = GetRectTransform(sendShuttleButton);
        RectTransform targetButtonRect = GetRectTransform(GetSendButton(shuttleIndex));

        if (templateButtonRect != null && targetButtonRect != null)
        {
            Vector2 buttonDelta = targetButtonRect.anchoredPosition - templateButtonRect.anchoredPosition;
            clonedRect.anchoredPosition = templateRoot.anchoredPosition + buttonDelta;
        }
        else
        {
            clonedRect.anchoredPosition = templateRoot.anchoredPosition + new Vector2(0f, -(shuttleIndex * (templateRoot.sizeDelta.y + 12f)));
        }

        return clonedRect;
    }

    private Button GetBoostOfferButton(int boostIndex)
    {
        if (boostIndex >= 0 && boostIndex < boostOfferButtons.Count)
        {
            return boostOfferButtons[boostIndex];
        }

        return null;
    }

    private Text GetBoostOfferButtonText(int boostIndex)
    {
        if (boostIndex >= 0 && boostIndex < boostOfferButtonTexts.Count)
        {
            return boostOfferButtonTexts[boostIndex];
        }

        return null;
    }

    private bool HasVisibleBoostOfferButtons()
    {
        for (int i = 0; i < boostOfferButtons.Count; i++)
        {
            if (boostOfferButtons[i] != null && boostOfferButtons[i].gameObject.activeSelf)
            {
                return true;
            }
        }

        return boostOfferButton != null && boostOfferButton.gameObject.activeSelf;
    }

    private void RegisterShuttleButtonSlot(
        int shuttleIndex,
        Button button,
        Text buttonText,
        Action<int> onShuttleSendRequested)
    {
        while (shuttleButtons.Count <= shuttleIndex)
        {
            shuttleButtons.Add(null);
            shuttleButtonTexts.Add(null);
        }

        shuttleButtons[shuttleIndex] = button;
        shuttleButtonTexts[shuttleIndex] = buttonText;

        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onShuttleSendRequested?.Invoke(shuttleIndex));
    }

    private void RegisterShuttleBarSlot(
        int shuttleIndex,
        RectTransform barRoot,
        Image fillImage,
        Text barText)
    {
        while (shuttleBarRoots.Count <= shuttleIndex)
        {
            shuttleBarRoots.Add(null);
            shuttleFillImages.Add(null);
            shuttleBarTexts.Add(null);
        }

        shuttleBarRoots[shuttleIndex] = barRoot;
        shuttleFillImages[shuttleIndex] = fillImage;
        shuttleBarTexts[shuttleIndex] = barText;
    }

    private RectTransform GetShuttleBarRoot(int shuttleIndex)
    {
        if (shuttleIndex >= 0 && shuttleIndex < shuttleBarRoots.Count && shuttleBarRoots[shuttleIndex] != null)
        {
            return shuttleBarRoots[shuttleIndex];
        }

        switch (shuttleIndex)
        {
            case 0:
                return GetBarRoot(shuttleFillImage, shuttleBarText);
            case 1:
                return GetBarRoot(shuttleFillImage2, shuttleBarText2);
            case 2:
                return GetBarRoot(shuttleFillImage3, shuttleBarText3);
            default:
                return null;
        }
    }

    private Image GetShuttleFillImage(int shuttleIndex)
    {
        if (shuttleIndex >= 0 && shuttleIndex < shuttleFillImages.Count && shuttleFillImages[shuttleIndex] != null)
        {
            return shuttleFillImages[shuttleIndex];
        }

        switch (shuttleIndex)
        {
            case 0:
                return shuttleFillImage;
            case 1:
                return shuttleFillImage2;
            case 2:
                return shuttleFillImage3;
            default:
                return null;
        }
    }

    private Text GetShuttleBarText(int shuttleIndex)
    {
        if (shuttleIndex >= 0 && shuttleIndex < shuttleBarTexts.Count && shuttleBarTexts[shuttleIndex] != null)
        {
            return shuttleBarTexts[shuttleIndex];
        }

        switch (shuttleIndex)
        {
            case 0:
                return shuttleBarText;
            case 1:
                return shuttleBarText2;
            case 2:
                return shuttleBarText3;
            default:
                return null;
        }
    }

    private Image FindBarFillImage(RectTransform barRoot)
    {
        if (barRoot == null)
        {
            return null;
        }

        Image[] images = barRoot.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null || images[i].transform == barRoot)
            {
                continue;
            }

            if (images[i].name.IndexOf("Fill", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return images[i];
            }
        }

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].transform != barRoot)
            {
                return images[i];
            }
        }

        return null;
    }

    private Button FindMenuButtonByName(string objectName)
    {
        if (dropdownMenuPanel == null)
        {
            return null;
        }

        Button[] buttons = dropdownMenuPanel.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return buttons[i];
            }
        }

        return null;
    }

    private Button CreateRuntimeMenuButton(string objectName)
    {
        if (dropdownMenuPanel == null)
        {
            return null;
        }

        Button[] menuButtons = dropdownMenuPanel.GetComponentsInChildren<Button>(true);
        Button templateButton = null;
        RectTransform lowestButtonRect = null;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null || menuButtons[i].transform.parent != dropdownMenuPanel.transform)
            {
                continue;
            }

            RectTransform buttonRect = menuButtons[i].GetComponent<RectTransform>();

            if (buttonRect == null)
            {
                continue;
            }

            templateButton = menuButtons[i];

            if (lowestButtonRect == null || buttonRect.anchoredPosition.y < lowestButtonRect.anchoredPosition.y)
            {
                lowestButtonRect = buttonRect;
            }
        }

        if (templateButton == null || lowestButtonRect == null)
        {
            return null;
        }

        GameObject clonedObject = Instantiate(templateButton.gameObject, dropdownMenuPanel.transform);
        clonedObject.name = objectName;
        clonedObject.transform.SetAsLastSibling();

        RectTransform templateRect = templateButton.GetComponent<RectTransform>();
        RectTransform clonedRect = clonedObject.GetComponent<RectTransform>();

        if (templateRect != null && clonedRect != null)
        {
            clonedRect.anchorMin = templateRect.anchorMin;
            clonedRect.anchorMax = templateRect.anchorMax;
            clonedRect.pivot = templateRect.pivot;
            clonedRect.sizeDelta = templateRect.sizeDelta;
            clonedRect.anchoredPosition = new Vector2(
                lowestButtonRect.anchoredPosition.x,
                lowestButtonRect.anchoredPosition.y - (lowestButtonRect.sizeDelta.y + 8f));
        }

        return clonedObject.GetComponent<Button>();
    }

    private string BuildMetalProductionText(GameData gameData)
    {
        if (gameData == null)
        {
            return "Produce Metal";
        }

        List<string> costParts = new List<string>();

        if (gameData.metalOreCost > 0)
        {
            costParts.Add(NumberFormatter.FormatInt(gameData.metalOreCost) + " Ore");
        }

        if (gameData.metalEnergyCost > 0)
        {
            costParts.Add(NumberFormatter.FormatInt(gameData.metalEnergyCost) + " Energy");
        }

        string costLine = costParts.Count > 0 ? string.Join(" + ", costParts) : "Free";
        return "Produce " + NumberFormatter.FormatInt(gameData.metalPerCraft) + " Metal\n" + costLine;
    }

    private string FormatTimer(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
    }
}
