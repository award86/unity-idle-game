using System;

[Serializable]
public class UpgradeEffectDefinition
{
    public UpgradeEffectType effectType = UpgradeEffectType.OrePerClick;
    public float valuePerLevel = 1f;
}
