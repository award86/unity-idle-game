using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text oreText;
    [SerializeField] private Text orePerSecondText;
    [SerializeField] private Text upgradeCostText;
    [SerializeField] private GameObject menuOverlayPanel;
    [SerializeField] private GameObject dropdownMenuPanel;
    [SerializeField] private GameObject resetConfirmationPanel;
    [SerializeField] private GameObject offlineRewardPanel;
    [SerializeField] private Text offlineRewardText;

    private void Awake()
    {
        HideMenuOverlay();
        HideMenu();
        HideResetConfirmation();
        HideOfflineReward();
    }

    public void UpdateUI(GameData gameData, int upgradeCost)
    {
        if (oreText != null)
        {
            oreText.text = "Ore: " + FormatNumber(gameData.ore);
        }

        if (orePerSecondText != null)
        {
            orePerSecondText.text = "Ore / sec: " + FormatNumber(gameData.orePerSecond);
        }

        if (upgradeCostText != null)
        {
            upgradeCostText.text = "Upgrade Cost: " + FormatNumber(upgradeCost);
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

    public void ShowResetConfirmation()
    {
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
        HideResetConfirmation();

        if (offlineRewardText != null)
        {
            offlineRewardText.text = "You earned " + FormatNumber(amount) + " Ore while offline.";
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

    private string FormatNumber(int value)
    {
        if (value >= GameSettings.NumberFormatB)
        {
            return (value / (float)GameSettings.NumberFormatB).ToString("0.#") + "B";
        }

        if (value >= GameSettings.NumberFormatM)
        {
            return (value / (float)GameSettings.NumberFormatM).ToString("0.#") + "M";
        }

        if (value >= GameSettings.NumberFormatK)
        {
            return (value / (float)GameSettings.NumberFormatK).ToString("0.#") + "K";
        }

        return value.ToString();
    }
}
