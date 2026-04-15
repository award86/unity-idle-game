using System;
using UnityEngine;
using UnityEngine.UI;

public class MetaBonusItemUI : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text costText;
    [SerializeField] private Text effectText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Text buyButtonText;

    private MetaBonusState metaBonusState;
    private Action<MetaBonusState> onMetaBonusBuyRequested;

    public void Initialize(MetaBonusState state, Action<MetaBonusState> onBuyRequested)
    {
        metaBonusState = state;
        onMetaBonusBuyRequested = onBuyRequested;
        SetupButton();
    }

    public void Refresh(GameData gameData, bool shouldShow)
    {
        if (metaBonusState == null || gameData == null)
        {
            return;
        }

        gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = metaBonusState.Definition.bonusName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = metaBonusState.Definition.description;
        }

        if (levelText != null)
        {
            levelText.text = metaBonusState.Definition.HasMaxLevel
                ? "Level: " + metaBonusState.Level + "/" + metaBonusState.Definition.maxLevel
                : "Level: " + metaBonusState.Level;
        }

        if (costText != null)
        {
            costText.text = metaBonusState.IsMaxLevel
                ? "Cost: MAX"
                : "Cost: " + NumberFormatter.FormatInt(metaBonusState.GetCurrentCrystalCost()) + " Crystal";
        }

        if (effectText != null)
        {
            effectText.text = BuildEffectText();
        }

        if (buyButton != null)
        {
            buyButton.interactable = !metaBonusState.IsMaxLevel && metaBonusState.CanAfford(gameData);
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = metaBonusState.IsMaxLevel ? "Max" : "Buy";
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

    private string BuildEffectText()
    {
        if (metaBonusState.Definition.Effects.Count <= 0)
        {
            return "Effect: None";
        }

        string[] lines = new string[metaBonusState.Definition.Effects.Count];

        for (int i = 0; i < metaBonusState.Definition.Effects.Count; i++)
        {
            UpgradeEffectDefinition effect = metaBonusState.Definition.Effects[i];
            float effectValue = metaBonusState.IsMaxLevel
                ? metaBonusState.GetCurrentEffectValue(effect)
                : metaBonusState.GetNextEffectValue(effect);
            lines[i] = EffectTextFormatter.BuildEffectLine(effect.effectType, effectValue);
        }

        return "Effect: " + string.Join("\n", lines);
    }

    private void HandleBuyClicked()
    {
        onMetaBonusBuyRequested?.Invoke(metaBonusState);
    }
}
