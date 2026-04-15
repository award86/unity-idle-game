using System.Collections.Generic;
using UnityEngine;

public class BuildingState
{
    public BuildingState(BuildingDefinition definition)
    {
        Definition = definition;
    }

    public BuildingDefinition Definition { get; }
    public int Level { get; private set; }
    public bool IsMaxLevel => Definition.HasMaxLevel && Level >= Definition.maxLevel;

    public List<ResourceAmount> GetCurrentCosts()
    {
        List<ResourceAmount> scaledCosts = new List<ResourceAmount>();

        for (int i = 0; i < Definition.BaseCosts.Count; i++)
        {
            ResourceAmount baseCost = Definition.BaseCosts[i];

            if (baseCost == null || baseCost.amount <= 0)
            {
                continue;
            }

            scaledCosts.Add(new ResourceAmount(baseCost.resourceType, GetScaledCost(baseCost.amount)));
        }

        return scaledCosts;
    }

    public bool CanAfford(GameData gameData)
    {
        List<ResourceAmount> costs = GetCurrentCosts();

        for (int i = 0; i < costs.Count; i++)
        {
            ResourceAmount cost = costs[i];

            if (gameData.GetResourceAmount(cost.resourceType) < cost.amount)
            {
                return false;
            }
        }

        return true;
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

    private int GetScaledCost(int baseAmount)
    {
        return Mathf.CeilToInt(baseAmount * Mathf.Pow(Definition.costMultiplier, Level));
    }
}
