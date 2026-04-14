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
    private TemporaryBoostState temporaryBoostState;
    private Action<UpgradeState> onUpgradeBuyRequested;
    private Action<TemporaryBoostState> onTemporaryBoostRequested;

    public void Initialize(UpgradeState state, Action<UpgradeState> onBuyRequested)
    {
        upgradeState = state;
        temporaryBoostState = null;
        onUpgradeBuyRequested = onBuyRequested;
        onTemporaryBoostRequested = null;

        SetupButton();
    }

    public void Initialize(TemporaryBoostState state, Action<TemporaryBoostState> onBuyRequested)
    {
        temporaryBoostState = state;
        upgradeState = null;
        onTemporaryBoostRequested = onBuyRequested;
        onUpgradeBuyRequested = null;

        SetupButton();
    }

    public void Refresh(int currentOre, int activeBoostCount)
    {
        if (upgradeState != null)
        {
            RefreshUpgrade(currentOre);
            return;
        }

        if (temporaryBoostState != null)
        {
            RefreshTemporaryBoost(activeBoostCount);
        }
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

    private void RefreshTemporaryBoost(int activeBoostCount)
    {
        if (temporaryBoostState == null)
        {
            return;
        }

        bool shouldShow = temporaryBoostState.ShouldShowInList;
        gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = temporaryBoostState.Definition.boostName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = temporaryBoostState.Definition.description;
        }

        if (levelText != null)
        {
            levelText.text = BuildTemporaryBoostStatusText();
        }

        if (costText != null)
        {
            costText.text = string.Empty;
        }

        if (effectText != null)
        {
            effectText.text = BuildTemporaryBoostEffectText();
        }

        if (buyButton != null)
        {
            buyButton.interactable = temporaryBoostState.IsAvailable &&
                                     activeBoostCount < GameSettings.MaxActiveTemporaryBoosts;
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = "Activate";
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
        if (upgradeState != null)
        {
            onUpgradeBuyRequested?.Invoke(upgradeState);
            return;
        }

        if (temporaryBoostState != null)
        {
            onTemporaryBoostRequested?.Invoke(temporaryBoostState);
        }
    }

    private string BuildTemporaryBoostStatusText()
    {
        switch (temporaryBoostState.Definition.availabilityType)
        {
            case TemporaryBoostAvailabilityType.ByTime:
                return "Boost: Ready every " + Mathf.RoundToInt(temporaryBoostState.Definition.appearanceIntervalSeconds) + "s";

            case TemporaryBoostAvailabilityType.ByAccumulatedOre:
                return "Boost: Ready every " + NumberFormatter.FormatInt(temporaryBoostState.Definition.oreRequiredForAppearance) + " earned Ore";

            default:
                return "Boost: Ready";
        }
    }

    private string BuildTemporaryBoostEffectText()
    {
        string targetText = temporaryBoostState.Definition.targetType == TemporaryBoostTargetType.OrePerClick
            ? "Ore / click"
            : "Ore / sec";

        return "Effect: x" + NumberFormatter.FormatFloat(temporaryBoostState.GetMultiplier()) +
               " " + targetText +
               " for " + Mathf.RoundToInt(temporaryBoostState.Definition.durationSeconds) + "s";
    }
}
