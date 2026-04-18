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

    public void Refresh(GameData gameData, UpgradeCategory selectedCategory, bool shouldDisplayUpgradeList)
    {
        RefreshUpgrade(gameData, selectedCategory, shouldDisplayUpgradeList);
    }

    private void SetupButton()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(HandleBuyClicked);
        }
    }

    private void RefreshUpgrade(GameData gameData, UpgradeCategory selectedCategory, bool shouldDisplayUpgradeList)
    {
        if (upgradeState == null || gameData == null)
        {
            return;
        }

        GameUiTextConfig uiText = GameTextProvider.UIText;

        bool shouldShow = shouldDisplayUpgradeList && upgradeState.Definition.ResolvedCategory == selectedCategory;
        gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = upgradeState.Definition.DisplayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = upgradeState.Definition.Description;
        }

        if (levelText != null)
        {
            levelText.text = upgradeState.Definition.HasMaxLevel
                ? uiText.LevelLabel + ": " + upgradeState.Level + "/" + upgradeState.Definition.maxLevel
                : uiText.LevelLabel + ": " + upgradeState.Level;
        }

        if (costText != null)
        {
            costText.text = upgradeState.IsMaxLevel
                ? uiText.CostLabel + ": " + uiText.MaxCostText
                : uiText.CostLabel + ": " + EffectTextFormatter.BuildCostText(upgradeState.GetCurrentCosts());
        }

        if (effectText != null)
        {
            effectText.text = BuildUpgradeEffectText();
        }

        if (buyButton != null)
        {
            buyButton.interactable = !upgradeState.IsMaxLevel && upgradeState.CanAfford(gameData);
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = upgradeState.IsMaxLevel ? uiText.MaxButtonText : uiText.BuyButtonText;
        }
    }

    private string BuildUpgradeEffectText()
    {
        GameUiTextConfig uiText = GameTextProvider.UIText;

        if (upgradeState.Definition.Effects.Count <= 0)
        {
            return uiText.EffectLabel + ": " + uiText.NoneText;
        }

        string[] effectLines = new string[upgradeState.Definition.Effects.Count];

        for (int i = 0; i < upgradeState.Definition.Effects.Count; i++)
        {
            effectLines[i] = BuildEffectLine(upgradeState.Definition.Effects[i]);
        }

        return uiText.EffectLabel + ": " + string.Join("\n", effectLines);
    }

    private string BuildEffectLine(UpgradeEffectDefinition effect)
    {
        float effectValue = upgradeState.IsMaxLevel
            ? upgradeState.GetCurrentEffectValue(effect)
            : upgradeState.GetNextEffectValue(effect);

        switch (effect.effectType)
        {
            default:
                return EffectTextFormatter.BuildEffectLine(effect.effectType, effectValue);
        }
    }

    private void HandleBuyClicked()
    {
        onUpgradeBuyRequested?.Invoke(upgradeState);
    }
}
