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

    private UpgradeState state;
    private Action<UpgradeState> onBuyRequested;

    public void Initialize(UpgradeState state, Action<UpgradeState> onBuyRequested)
    {
        this.state = state;
        this.onBuyRequested = onBuyRequested;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(HandleBuyClicked);
        }
    }

    public void Refresh(int currentOre)
    {
        if (state == null)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = state.Definition.upgradeName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = state.Definition.description;
        }

        if (levelText != null)
        {
            levelText.text = state.Definition.HasMaxLevel
                ? "Level: " + state.Level + "/" + state.Definition.maxLevel
                : "Level: " + state.Level;
        }

        if (costText != null)
        {
            costText.text = state.IsMaxLevel
                ? "Cost: MAX"
                : "Cost: " + NumberFormatter.FormatInt(state.GetCurrentCost());
        }

        if (effectText != null)
        {
            effectText.text = BuildEffectText();
        }

        if (buyButton != null)
        {
            buyButton.interactable = !state.IsMaxLevel && currentOre >= state.GetCurrentCost();
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = state.IsMaxLevel ? "Max" : "Buy";
        }
    }

    private string BuildEffectText()
    {
        float value = state.IsMaxLevel ? state.GetCurrentEffectValue() : state.GetNextLevelValue();

        switch (state.Definition.effectType)
        {
            case UpgradeEffectType.OrePerClickFlat:
                return "Effect: +" + NumberFormatter.FormatFloat(value) + " Ore / click";

            case UpgradeEffectType.OrePerClickMultiplier:
                return "Effect: +" + NumberFormatter.FormatFloat(value * 100f) + "% Ore / click";

            case UpgradeEffectType.OrePerSecondFlat:
                return "Effect: +" + NumberFormatter.FormatFloat(value) + " Ore / sec";

            case UpgradeEffectType.OrePerSecondMultiplier:
                return "Effect: +" + NumberFormatter.FormatFloat(value * 100f) + "% Ore / sec";

            case UpgradeEffectType.TemporaryIncomeMultiplier:
                return "Effect: x" + NumberFormatter.FormatFloat(value) + " Income for " + Mathf.RoundToInt(state.Definition.durationSeconds) + "s";

            default:
                return "Effect: Unknown";
        }
    }

    private void HandleBuyClicked()
    {
        onBuyRequested?.Invoke(state);
    }
}
