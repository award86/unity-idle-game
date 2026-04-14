using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text oreText;
    [SerializeField] private Text orePerSecondText;

    [FormerlySerializedAs("upgradeCostText")]
    [SerializeField] private Text orePerClickText;

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

    private readonly List<UpgradeItemUI> upgradeItems = new List<UpgradeItemUI>();

    private void Awake()
    {
        HideMenuOverlay();
        HideMenu();
        HideUpgradePanel();
        HideResetConfirmation();
        HideOfflineReward();
        SetMainScreenUpgradeButtonVisible(false);
        UpdateBoostUI(null);
    }

    public void UpdateUI(GameData gameData)
    {
        if (oreText != null)
        {
            oreText.text = "Ore: " + NumberFormatter.FormatInt(gameData.ore);
        }

        if (orePerSecondText != null)
        {
            orePerSecondText.text = "Ore / sec: " + NumberFormatter.FormatInt(gameData.orePerSecond);
        }

        if (orePerClickText != null)
        {
            orePerClickText.text = "Ore / click: " + NumberFormatter.FormatInt(gameData.orePerClick);
        }
    }

    public void InitializeUpgradeList(
        IReadOnlyList<UpgradeState> upgradeStates,
        IReadOnlyList<TemporaryBoostState> temporaryBoostStates,
        Action<UpgradeState> onUpgradeBuyRequested,
        Action<TemporaryBoostState> onTemporaryBoostRequested)
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

        if (temporaryBoostStates != null)
        {
            for (int i = 0; i < temporaryBoostStates.Count; i++)
            {
                UpgradeItemUI item = Instantiate(upgradeItemPrefab, upgradeListRoot);
                item.Initialize(temporaryBoostStates[i], onTemporaryBoostRequested);
                upgradeItems.Add(item);
            }
        }

        RefreshUpgradeList(0, 0);
    }

    public void RefreshUpgradeList(int currentOre, int activeBoostCount)
    {
        for (int i = 0; i < upgradeItems.Count; i++)
        {
            upgradeItems[i].Refresh(currentOre, activeBoostCount);
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

    public void ShowOfflineReward(int amount)
    {
        HideMenu();
        HideUpgradePanel();
        HideResetConfirmation();

        if (offlineRewardText != null)
        {
            offlineRewardText.text = "You earned " + NumberFormatter.FormatInt(amount) + " Ore while offline.";
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
}
