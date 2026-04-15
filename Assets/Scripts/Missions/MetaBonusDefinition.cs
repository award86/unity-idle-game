using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MetaBonusDefinition
{
    public string id = "new_meta_bonus";
    public string bonusName = "New Meta Bonus";

    [TextArea]
    public string description = "Meta bonus description";

    public int crystalCost = 1;
    public float costMultiplier = 1.6f;
    public int maxLevel = 1;
    public List<UpgradeEffectDefinition> effects = new List<UpgradeEffectDefinition>();

    public IReadOnlyList<UpgradeEffectDefinition> Effects => effects;
    public bool HasMaxLevel => maxLevel > 0;
}
