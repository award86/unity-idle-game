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
    [SerializeField] private GameObject boostOfferPanel;
    [SerializeField] private Text boostOfferNameText;
    [SerializeField] private Text boostOfferDescriptionText;
    [SerializeField] private Text boostOfferEffectText;

    private readonly List<UpgradeItemUI> upgradeItems = new List<UpgradeItemUI>();
    private readonly List<BuildingItemUI> buildingItems = new List<BuildingItemUI>();
    private readonly List<MetaBonusItemUI> metaBonusItems = new List<MetaBonusItemUI>();
    private readonly List<Button> shuttleButtons = new List<Button>();
    private readonly List<Text> shuttleButtonTexts = new List<Text>();
    private GameData lastDisplayedGameData;
    private UpgradeCategory selectedUpgradeCategory = UpgradeCategory.Miner;
    private float displayedEnergyAmount;
    private bool hasDisplayedEnergyAmount;

    public bool IsOfflineRewardVisible => offlineRewardPanel != null && offlineRewardPanel.activeSelf;
    public bool IsBoostOfferVisible => boostOfferPanel != null && boostOfferPanel.activeSelf;
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

    private void Update()
    {
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

        if (shuttleText != null)
        {
            bool shouldUseLegacyShuttleText = !IsShuttleBarActive();
            shuttleText.gameObject.SetActive(shouldUseLegacyShuttleText);

            if (shouldUseLegacyShuttleText)
            {
                shuttleText.text =
                    "Shuttle: " +
                    NumberFormatter.FormatInt(GetDisplayedShuttleLoad(gameData)) +
                    " / " +
                    NumberFormatter.FormatInt(gameData.shuttleCapacity * gameData.ActiveShuttleCount);
            }
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
        UpdateShuttleBar(gameData);

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
            return;
        }

        HideMenu();
        HideAllContentPanels();
        HideResetConfirmation();

        if (boostOfferNameText != null)
        {
            boostOfferNameText.text = boostState.Definition.boostName;
        }

        if (boostOfferDescriptionText != null)
        {
            boostOfferDescriptionText.text = boostState.Definition.description;
        }

        if (boostOfferEffectText != null)
        {
            boostOfferEffectText.text = BuildBoostOfferEffectText(boostState);
        }

        if (boostOfferPanel != null)
        {
            boostOfferPanel.SetActive(true);
        }

        SetSharedOverlayVisible(true);
    }

    public void HideBoostOffer()
    {
        if (boostOfferPanel != null)
        {
            boostOfferPanel.SetActive(false);
        }

        SetSharedOverlayVisible(false);
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

        if (shuttleText != null)
        {
            shuttleText.gameObject.SetActive(!IsShuttleBarActive());
        }
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

    private string BuildBoostOfferEffectText(TemporaryBoostState boostState)
    {
        string targetText = boostState.Definition.targetType == TemporaryBoostTargetType.OrePerClick
            ? "Ore / click"
            : "Ore / sec";

        return "Effect: x" +
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
        return IsBarActive(shuttleFillImage, shuttleBarText);
    }

    private bool IsBarActive(Graphic fillGraphic, Graphic textGraphic)
    {
        return IsGraphicActive(fillGraphic) || IsGraphicActive(textGraphic);
    }

    private bool IsGraphicActive(Graphic graphic)
    {
        return graphic != null && graphic.gameObject.activeInHierarchy;
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

    private void UpdateShuttleBar(GameData gameData)
    {
        if (gameData == null)
        {
            return;
        }

        int currentShuttleOre = GetDisplayedShuttleLoad(gameData);
        int maxShuttleOre = gameData.shuttleCapacity * gameData.ActiveShuttleCount;

        if (shuttleFillImage != null)
        {
            shuttleFillImage.fillAmount = maxShuttleOre > 0
                ? Mathf.Clamp01(currentShuttleOre / (float)maxShuttleOre)
                : 0f;
        }

        if (shuttleBarText != null)
        {
            shuttleBarText.text =
                NumberFormatter.FormatInt(currentShuttleOre) +
                " / " +
                NumberFormatter.FormatInt(maxShuttleOre);
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
