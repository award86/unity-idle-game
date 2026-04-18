using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingItemUI : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text costText;
    [SerializeField] private Text effectText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Text buyButtonText;

    private BuildingState buildingState;
    private Action<BuildingState> onBuildingBuyRequested;

    public void Initialize(BuildingState state, Action<BuildingState> onBuyRequested)
    {
        buildingState = state;
        onBuildingBuyRequested = onBuyRequested;
        SetupButton();
    }

    public void Refresh(GameData gameData, bool shouldShow)
    {
        if (buildingState == null || gameData == null)
        {
            return;
        }

        GameUiTextConfig uiText = GameTextProvider.UIText;

        gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = buildingState.Definition.DisplayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = buildingState.Definition.Description;
        }

        if (levelText != null)
        {
            levelText.text = buildingState.Definition.HasMaxLevel
                ? uiText.LevelLabel + ": " + buildingState.Level + "/" + buildingState.Definition.maxLevel
                : uiText.LevelLabel + ": " + buildingState.Level;
        }

        if (costText != null)
        {
            costText.text = buildingState.IsMaxLevel
                ? uiText.CostLabel + ": " + uiText.MaxCostText
                : uiText.CostLabel + ": " + EffectTextFormatter.BuildCostText(buildingState.GetCurrentCosts());
        }

        if (effectText != null)
        {
            effectText.text = BuildBuildingEffectText();
        }

        if (buyButton != null)
        {
            buyButton.interactable = !buildingState.IsMaxLevel && buildingState.CanAfford(gameData);
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = buildingState.IsMaxLevel ? uiText.MaxButtonText : uiText.BuildButtonText;
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

    private string BuildBuildingEffectText()
    {
        GameUiTextConfig uiText = GameTextProvider.UIText;

        if (buildingState.Definition.Effects.Count <= 0)
        {
            return uiText.EffectLabel + ": " + uiText.NoneText;
        }

        string[] effectLines = new string[buildingState.Definition.Effects.Count];

        for (int i = 0; i < buildingState.Definition.Effects.Count; i++)
        {
            effectLines[i] = BuildEffectLine(buildingState.Definition.Effects[i]);
        }

        return uiText.EffectLabel + ": " + string.Join("\n", effectLines);
    }

    private string BuildEffectLine(UpgradeEffectDefinition effect)
    {
        float effectValue = buildingState.IsMaxLevel
            ? buildingState.GetCurrentEffectValue(effect)
            : buildingState.GetNextEffectValue(effect);

        switch (effect.effectType)
        {
            default:
                return EffectTextFormatter.BuildEffectLine(effect.effectType, effectValue);
        }
    }

    private void HandleBuyClicked()
    {
        onBuildingBuyRequested?.Invoke(buildingState);
    }
}
