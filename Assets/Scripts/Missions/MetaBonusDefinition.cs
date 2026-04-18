using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MetaBonusDefinition
{
    public string id = "new_meta_bonus";
    public string bonusName = "New Meta Bonus";
    public string bonusNameRu = "";

    [TextArea]
    public string description = "Meta bonus description";
    [TextArea]
    public string descriptionRu = "";

    public int crystalCost = 1;
    public float costMultiplier = 1.6f;
    public int maxLevel = 1;
    public List<UpgradeEffectDefinition> effects = new List<UpgradeEffectDefinition>();

    public IReadOnlyList<UpgradeEffectDefinition> Effects => effects;
    public bool HasMaxLevel => maxLevel > 0;
    public string DisplayName => GameTextProvider.GetText(bonusName, bonusNameRu);
    public string Description => GameTextProvider.GetText(description, descriptionRu);
}
