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

        return Definition.valuePerLevel * Level;
    }

    public float GetNextLevelValue()
    {
        return Definition.valuePerLevel * (Level + 1);
    }

    public float GetCurrentShuttleTravelTimeReduction()
    {
        if (Level <= 0)
        {
            return 0f;
        }

        return Definition.shuttleTravelTimeReductionPerLevel * Level;
    }

    public float GetNextShuttleTravelTimeReduction()
    {
        return Definition.shuttleTravelTimeReductionPerLevel * (Level + 1);
    }

    public int GetCurrentShuttleCapacityIncrease()
    {
        if (Level <= 0)
        {
            return 0;
        }

        return Definition.shuttleCapacityIncreasePerLevel * Level;
    }

    public int GetNextShuttleCapacityIncrease()
    {
        return Definition.shuttleCapacityIncreasePerLevel * (Level + 1);
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
