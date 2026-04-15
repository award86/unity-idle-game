using UnityEngine;

public class MetaBonusState
{
    public MetaBonusState(MetaBonusDefinition definition)
    {
        Definition = definition;
    }

    public MetaBonusDefinition Definition { get; }
    public int Level { get; private set; }
    public bool IsMaxLevel => Definition.HasMaxLevel && Level >= Definition.maxLevel;

    public int GetCurrentCrystalCost()
    {
        return Mathf.CeilToInt(Definition.crystalCost * Mathf.Pow(Definition.costMultiplier, Level));
    }

    public bool CanAfford(GameData gameData)
    {
        return gameData != null && gameData.crystal >= GetCurrentCrystalCost();
    }

    public float GetCurrentEffectValue(UpgradeEffectDefinition effect)
    {
        if (effect == null || Level <= 0)
        {
            return 0f;
        }

        return effect.valuePerLevel * Level;
    }

    public float GetNextEffectValue(UpgradeEffectDefinition effect)
    {
        if (effect == null)
        {
            return 0f;
        }

        return effect.valuePerLevel * (Level + 1);
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
