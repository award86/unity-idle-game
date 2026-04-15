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
    [SerializeField] private Text shuttleText;
    [SerializeField] private Text orePerSecondText;

    [FormerlySerializedAs("upgradeCostText")]
    [SerializeField] private Text orePerClickText;
    [SerializeField] private Image energyFillImage;
    [SerializeField] private Text energyBarText;
    [SerializeField] private Button sendShuttleButton;
    [SerializeField] private Text sendShuttleButtonText;
    [SerializeField] private Button produceMetalButton;
    [SerializeField] private Text produceMetalButtonText;

    [FormerlySerializedAs("menuOverlayPanel")]
    [FormerlySerializedAs("boostOfferOverlayPanel")]
    [SerializeField] private GameObject sharedOverlayPanel;
    [SerializeField] private GameObject dropdownMenuPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject mainScreenUpgradeButton;
    [SerializeField] private GameObject mainScreenBuildButton;
    [SerializeField] private Transform upgradeListRoot;
    [SerializeField] private UpgradeItemUI upgradeItemPrefab;
    [SerializeField] private BuildingItemUI buildingItemPrefab;
    [SerializeField] private Text boostStatusText;
    [SerializeField] private GameObject resetConfirmationPanel;
    [SerializeField] private GameObject offlineRewardPanel;
    [SerializeField] private Text offlineRewardText;
    [SerializeField] private GameObject boostOfferPanel;
    [SerializeField] private Text boostOfferNameText;
    [SerializeField] private Text boostOfferDescriptionText;
    [SerializeField] private Text boostOfferEffectText;

    private readonly List<UpgradeItemUI> upgradeItems = new List<UpgradeItemUI>();
    private readonly List<BuildingItemUI> buildingItems = new List<BuildingItemUI>();
    private GameData lastDisplayedGameData;
    private UpgradeCategory selectedUpgradeCategory = UpgradeCategory.Miner;
    private PanelContentMode currentPanelContentMode = PanelContentMode.Upgrades;

    public bool IsOfflineRewardVisible => offlineRewardPanel != null && offlineRewardPanel.activeSelf;
    public bool IsBoostOfferVisible => boostOfferPanel != null && boostOfferPanel.activeSelf;
    public bool IsUpgradePanelVisible => upgradePanel != null && upgradePanel.activeSelf;
    public bool IsBuildingPanelMode => currentPanelContentMode == PanelContentMode.Buildings;
    public bool IsMenuVisible => dropdownMenuPanel != null && dropdownMenuPanel.activeSelf;
    public bool IsResetConfirmationVisible => resetConfirmationPanel != null && resetConfirmationPanel.activeSelf;
    public bool IsBusyWithOtherWindow =>
        IsOfflineRewardVisible ||
        IsUpgradePanelVisible ||
        IsMenuVisible ||
        IsResetConfirmationVisible;

    private void Awake()
    {
        HideSharedOverlay();
        HideMenu();
        HideUpgradePanel();
        HideResetConfirmation();
        HideOfflineReward();
        HideBoostOffer();
        SetMainScreenUpgradeButtonVisible(false);
        SetMainScreenBuildButtonVisible(false);
        UpdateBoostUI(null);
    }

    public void UpdateUI(GameData gameData)
    {
        lastDisplayedGameData = gameData;

        if (oreText != null)
        {
            oreText.text = "Warehouse Ore: " + NumberFormatter.FormatInt(gameData.ore);
        }

        if (energyText != null)
        {
            energyText.text = "Energy: " + NumberFormatter.FormatInt(gameData.energy) + " / " + NumberFormatter.FormatInt(gameData.energyMax);
        }

        if (metalText != null)
        {
            metalText.text = "Metal: " + NumberFormatter.FormatInt(gameData.metal);
        }

        if (crystalText != null)
        {
            crystalText.text = "Crystal: " + NumberFormatter.FormatInt(gameData.crystal);
        }

        if (shuttleText != null)
        {
            shuttleText.text =
                "Platform: " +
                NumberFormatter.FormatInt(gameData.shuttleOre) +
                " / " +
                NumberFormatter.FormatInt(gameData.platformCapacity);
        }

        if (orePerSecondText != null)
        {
            orePerSecondText.text = "Ore / sec: " + NumberFormatter.FormatInt(gameData.orePerSecond);
        }

        if (orePerClickText != null)
        {
            orePerClickText.text = "Ore / click: " + NumberFormatter.FormatInt(gameData.orePerClick);
        }

        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = gameData.energyMax > 0
                ? Mathf.Clamp01(gameData.energy / (float)gameData.energyMax)
                : 0f;
        }

        if (energyBarText != null)
        {
            energyBarText.text = NumberFormatter.FormatInt(gameData.energy) + " / " + NumberFormatter.FormatInt(gameData.energyMax);
        }

        if (sendShuttleButton != null)
        {
            sendShuttleButton.interactable = gameData.shuttleSendCooldownRemaining <= 0f;
        }

        if (sendShuttleButtonText != null)
        {
            sendShuttleButtonText.text = gameData.shuttleSendCooldownRemaining > 0f
                ? "Send " + FormatTimer(gameData.shuttleSendCooldownRemaining)
                : "Send";
        }

        if (produceMetalButton != null)
        {
            produceMetalButton.interactable = CanProduceMetal(gameData);
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

        RefreshUpgradeList(lastDisplayedGameData ?? new GameData());
    }

    public void RefreshUpgradeList(GameData gameData)
    {
        lastDisplayedGameData = gameData;

        for (int i = 0; i < upgradeItems.Count; i++)
        {
            upgradeItems[i].Refresh(gameData, selectedUpgradeCategory, currentPanelContentMode == PanelContentMode.Upgrades);
        }
    }

    public void InitializeBuildingList(
        IReadOnlyList<BuildingState> buildingStates,
        Action<BuildingState> onBuildingBuyRequested)
    {
        ClearBuildingList();

        if (upgradeListRoot == null || buildingItemPrefab == null)
        {
            return;
        }

        if (buildingStates != null)
        {
            for (int i = 0; i < buildingStates.Count; i++)
            {
                BuildingItemUI item = Instantiate(buildingItemPrefab, upgradeListRoot);
                item.Initialize(buildingStates[i], onBuildingBuyRequested);
                buildingItems.Add(item);
            }
        }

        RefreshBuildingList(lastDisplayedGameData ?? new GameData());
    }

    public void RefreshBuildingList(GameData gameData)
    {
        lastDisplayedGameData = gameData;

        bool shouldShowBuildings = currentPanelContentMode == PanelContentMode.Buildings;

        for (int i = 0; i < buildingItems.Count; i++)
        {
            buildingItems[i].Refresh(gameData, shouldShowBuildings);
        }
    }

    public void SetUpgradeCategory(UpgradeCategory category)
    {
        selectedUpgradeCategory = category;
        RefreshUpgradeList(lastDisplayedGameData ?? new GameData());
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

    public void ToggleUpgradePanel()
    {
        if (upgradePanel == null)
        {
            return;
        }

        upgradePanel.SetActive(!upgradePanel.activeSelf);
        RefreshVisiblePanelMode();
    }

    public void OpenUpgradePanel()
    {
        if (upgradePanel == null)
        {
            return;
        }

        bool wasVisibleInSameMode =
            upgradePanel.activeSelf &&
            currentPanelContentMode == PanelContentMode.Upgrades;

        currentPanelContentMode = PanelContentMode.Upgrades;

        if (wasVisibleInSameMode)
        {
            upgradePanel.SetActive(false);
            RefreshVisiblePanelMode();
            return;
        }

        upgradePanel.SetActive(true);
        RefreshVisiblePanelMode();
    }

    public void OpenBuildingPanel()
    {
        if (upgradePanel == null)
        {
            return;
        }

        bool wasVisibleInSameMode =
            upgradePanel.activeSelf &&
            currentPanelContentMode == PanelContentMode.Buildings;

        currentPanelContentMode = PanelContentMode.Buildings;

        if (wasVisibleInSameMode)
        {
            upgradePanel.SetActive(false);
            RefreshVisiblePanelMode();
            return;
        }

        upgradePanel.SetActive(true);
        RefreshVisiblePanelMode();
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        RefreshVisiblePanelMode();
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

    public void ShowResetConfirmation()
    {
        HideUpgradePanel();

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
        HideUpgradePanel();
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
        HideUpgradePanel();
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

    private void RefreshVisiblePanelMode()
    {
        RefreshUpgradeList(lastDisplayedGameData ?? new GameData());
        RefreshBuildingList(lastDisplayedGameData ?? new GameData());
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

    private enum PanelContentMode
    {
        Upgrades,
        Buildings
    }
}
