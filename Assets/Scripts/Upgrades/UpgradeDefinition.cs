using System;
using UnityEngine;

[Serializable]
public class UpgradeDefinition
{
    public string id = "new_upgrade";
    public string upgradeName = "New Upgrade";

    [TextArea]
    public string description = "Upgrade description";

    public UpgradeEffectType effectType = UpgradeEffectType.MiningPerClick;
    public int baseCost = 10;
    public float costMultiplier = 1.5f;
    public float baseValue = 1f;
    public float valuePerLevel = 0f;
    public int maxLevel = 0;

    public bool HasMaxLevel => maxLevel > 0;
}
