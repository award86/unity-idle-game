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
        UpdateBoostUI(false, string.Empty, 1f, 0f);
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

    public void InitializeUpgradeList(IReadOnlyList<UpgradeState> upgradeStates, Action<UpgradeState> onBuyRequested)
    {
        ClearUpgradeList();

        if (upgradeListRoot == null || upgradeItemPrefab == null || upgradeStates == null)
        {
            return;
        }

        for (int i = 0; i < upgradeStates.Count; i++)
        {
            UpgradeItemUI item = Instantiate(upgradeItemPrefab, upgradeListRoot);
            item.Initialize(upgradeStates[i], onBuyRequested);
            upgradeItems.Add(item);
        }

        RefreshUpgradeList(upgradeStates, 0);
    }

    public void RefreshUpgradeList(IReadOnlyList<UpgradeState> upgradeStates, int currentOre)
    {
        if (upgradeStates == null)
        {
            return;
        }

        int itemCount = Mathf.Min(upgradeItems.Count, upgradeStates.Count);

        for (int i = 0; i < itemCount; i++)
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

    public void UpdateBoostUI(bool hasActiveBoost, string boostName, float multiplier, float remainingTime)
    {
        if (boostStatusText == null)
        {
            return;
        }

        boostStatusText.gameObject.SetActive(hasActiveBoost);

        if (!hasActiveBoost)
        {
            boostStatusText.text = string.Empty;
            return;
        }

        boostStatusText.text = boostName + " x" + NumberFormatter.FormatFloat(multiplier) + " - " + Mathf.CeilToInt(remainingTime) + "s";
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
