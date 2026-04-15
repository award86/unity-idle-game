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

        bool shouldShow = shouldDisplayUpgradeList && upgradeState.Definition.ResolvedCategory == selectedCategory;
        gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

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
                : "Cost: " + EffectTextFormatter.BuildCostText(upgradeState.GetCurrentCosts());
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
            buyButtonText.text = upgradeState.IsMaxLevel ? "Max" : "Buy";
        }
    }

    private string BuildUpgradeEffectText()
    {
        if (upgradeState.Definition.Effects.Count <= 0)
        {
            return "Effect: None";
        }

        string[] effectLines = new string[upgradeState.Definition.Effects.Count];

        for (int i = 0; i < upgradeState.Definition.Effects.Count; i++)
        {
            effectLines[i] = BuildEffectLine(upgradeState.Definition.Effects[i]);
        }

        return "Effect: " + string.Join("\n", effectLines);
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
