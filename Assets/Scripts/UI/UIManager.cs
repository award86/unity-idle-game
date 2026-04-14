using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text oreText;
    [SerializeField] private Text shuttleText;
    [SerializeField] private Text orePerSecondText;

    [FormerlySerializedAs("upgradeCostText")]
    [SerializeField] private Text orePerClickText;
    [SerializeField] private Button sendShuttleButton;
    [SerializeField] private Text sendShuttleButtonText;

    [SerializeField] private GameObject menuOverlayPanel;
    [SerializeField] private GameObject dropdownMenuPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject mainScreenUpgradeButton;
    [SerializeField] private Transform upgradeListRoot;
    [SerializeField] private UpgradeItemUI upgradeItemPrefab;
    [SerializeField] private Text boostStatusText;
    [SerializeField] private GameObject resetConfirmationPanel;
    [SerializeField] private GameObject offlineRewardPanel;
    [SerializeField] private Text offlineRewardText;
    [SerializeField] private GameObject boostOfferPanel;
    [SerializeField] private Text boostOfferNameText;
    [SerializeField] private Text boostOfferDescriptionText;
    [SerializeField] private Text boostOfferEffectText;

    private readonly List<UpgradeItemUI> upgradeItems = new List<UpgradeItemUI>();

    public bool IsOfflineRewardVisible => offlineRewardPanel != null && offlineRewardPanel.activeSelf;
    public bool IsBoostOfferVisible => boostOfferPanel != null && boostOfferPanel.activeSelf;

    private void Awake()
    {
        HideMenuOverlay();
        HideMenu();
        HideUpgradePanel();
        HideResetConfirmation();
        HideOfflineReward();
        HideBoostOffer();
        SetMainScreenUpgradeButtonVisible(false);
        UpdateBoostUI(null);
    }

    public void UpdateUI(GameData gameData)
    {
        if (oreText != null)
        {
            oreText.text = "Warehouse Ore: " + NumberFormatter.FormatInt(gameData.ore);
        }

        if (shuttleText != null)
        {
            shuttleText.text =
                "Shuttle: " +
                NumberFormatter.FormatInt(gameData.shuttleOre) +
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

        RefreshUpgradeList(0);
    }

    public void RefreshUpgradeList(int currentOre)
    {
        for (int i = 0; i < upgradeItems.Count; i++)
        {
            upgradeItems[i].Refresh(currentOre);
        }
    }

    public void ToggleMenu()
    {
        if (dropdownMenuPanel == null)
        {
            return;
        }

        bool shouldShowMenu = !dropdownMenuPanel.activeSelf;
        dropdownMenuPanel.SetActive(shouldShowMenu);
        SetMenuOverlayVisible(shouldShowMenu);

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

        HideMenuOverlay();
    }

    public void ToggleUpgradePanel()
    {
        if (upgradePanel == null)
        {
            return;
        }

        upgradePanel.SetActive(!upgradePanel.activeSelf);
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    public void SetMainScreenUpgradeButtonVisible(bool isVisible)
    {
        if (mainScreenUpgradeButton != null)
        {
            mainScreenUpgradeButton.SetActive(isVisible);
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
    }

    public void HideBoostOffer()
    {
        if (boostOfferPanel != null)
        {
            boostOfferPanel.SetActive(false);
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

    private void SetMenuOverlayVisible(bool isVisible)
    {
        if (menuOverlayPanel != null)
        {
            menuOverlayPanel.SetActive(isVisible);
        }
    }

    private void HideMenuOverlay()
    {
        SetMenuOverlayVisible(false);
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

    private string FormatTimer(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
    }
}
