using System;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text costText;
    [SerializeField] private Text effectText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Text buyButtonText;

    private UpgradeState upgradeState;
    private Action<UpgradeState> onUpgradeBuyRequested;

    public void Initialize(UpgradeState state, Action<UpgradeState> onBuyRequested)
    {
        upgradeState = state;
        onUpgradeBuyRequested = onBuyRequested;
        SetupButton();
    }

    public void Refresh(int currentOre)
    {
        RefreshUpgrade(currentOre);
    }

    private void SetupButton()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(HandleBuyClicked);
        }
    }

    private void RefreshUpgrade(int currentOre)
    {
        if (upgradeState == null)
        {
            return;
        }

        gameObject.SetActive(true);

        if (nameText != null)
        {
            nameText.text = upgradeState.Definition.upgradeName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = upgradeState.Definition.description;
        }

        if (levelText != null)
        {
            levelText.text = upgradeState.Definition.HasMaxLevel
                ? "Level: " + upgradeState.Level + "/" + upgradeState.Definition.maxLevel
                : "Level: " + upgradeState.Level;
        }

        if (costText != null)
        {
            costText.text = upgradeState.IsMaxLevel
                ? "Cost: MAX"
                : "Cost: " + NumberFormatter.FormatInt(upgradeState.GetCurrentCost());
        }

        if (effectText != null)
        {
            effectText.text = BuildUpgradeEffectText();
        }

        if (buyButton != null)
        {
            buyButton.interactable = !upgradeState.IsMaxLevel && currentOre >= upgradeState.GetCurrentCost();
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = upgradeState.IsMaxLevel ? "Max" : "Buy";
        }
    }

    private string BuildUpgradeEffectText()
    {
        switch (upgradeState.Definition.effectType)
        {
            case UpgradeEffectType.MiningPerClick:
                float clickValue = upgradeState.IsMaxLevel ? upgradeState.GetCurrentEffectValue() : upgradeState.GetNextLevelValue();
                return "Effect: +" + NumberFormatter.FormatFloat(clickValue) + " Ore / click";

            case UpgradeEffectType.MiningPerSecond:
                float passiveValue = upgradeState.IsMaxLevel ? upgradeState.GetCurrentEffectValue() : upgradeState.GetNextLevelValue();
                return "Effect: +" + NumberFormatter.FormatFloat(passiveValue) + " Ore / sec";

            case UpgradeEffectType.Shuttle:
                return BuildShuttleEffectText();

            case UpgradeEffectType.ShuttleAutoSend:
                return "Effect: Auto send when shuttle is full";

            default:
                return "Effect: Unknown";
        }
    }

    private string BuildShuttleEffectText()
    {
        float travelTimeReduction = upgradeState.IsMaxLevel
            ? upgradeState.GetCurrentShuttleTravelTimeReduction()
            : upgradeState.GetNextShuttleTravelTimeReduction();
        int capacityIncrease = upgradeState.IsMaxLevel
            ? upgradeState.GetCurrentShuttleCapacityIncrease()
            : upgradeState.GetNextShuttleCapacityIncrease();

        bool hasTravelBonus = travelTimeReduction > 0f;
        bool hasCapacityBonus = capacityIncrease > 0;

        if (hasTravelBonus && hasCapacityBonus)
        {
            return "Effect: -" + NumberFormatter.FormatFloat(travelTimeReduction) +
                   "s travel time, +" + NumberFormatter.FormatInt(capacityIncrease) + " capacity";
        }

        if (hasTravelBonus)
        {
            return "Effect: -" + NumberFormatter.FormatFloat(travelTimeReduction) + "s travel time";
        }

        if (hasCapacityBonus)
        {
            return "Effect: +" + NumberFormatter.FormatInt(capacityIncrease) + " capacity";
        }

        return "Effect: Shuttle upgrade";
    }

    private void HandleBuyClicked()
    {
        onUpgradeBuyRequested?.Invoke(upgradeState);
    }
}
