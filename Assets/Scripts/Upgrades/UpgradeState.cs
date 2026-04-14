using UnityEngine;

public class UpgradeState
{
    public UpgradeState(UpgradeDefinition definition)
    {
        Definition = definition;
    }

    public UpgradeDefinition Definition { get; }
    public int Level { get; private set; }
    public bool IsMaxLevel => Definition.HasMaxLevel && Level >= Definition.maxLevel;

    public int GetCurrentCost()
    {
        return Mathf.CeilToInt(Definition.baseCost * Mathf.Pow(Definition.costMultiplier, Level));
    }

    public float GetCurrentEffectValue()
    {
        if (Level <= 0)
        {
            return 0f;
        }

        return Definition.baseValue + (Definition.valuePerLevel * (Level - 1));
    }

    public float GetNextLevelValue()
    {
        return Definition.baseValue + (Definition.valuePerLevel * Level);
    }

    public void SetLevel(int level)
    {
        Level = Mathf.Max(0, level);
    }

    public void IncreaseLevel()
    {
        Level += 1;
    }
}
