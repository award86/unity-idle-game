using System.Collections.Generic;
using UnityEngine;

public class ResourceSystem
{
    private readonly GameData gameData;
    private readonly PlatformSystem platformSystem;

    public ResourceSystem(GameData gameData, PlatformSystem platformSystem)
    {
        this.gameData = gameData;
        this.platformSystem = platformSystem;
        gameData.EnsureDefaultResources();
    }

    public int GetAmount(ResourceType resourceType)
    {
        return gameData.GetResourceAmount(resourceType);
    }

    public int AddResource(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        if (resourceType == ResourceType.Energy)
        {
            int currentEnergy = gameData.energy;
            int maxEnergy = Mathf.Max(0, gameData.energyMax);
            int addedEnergy = Mathf.Min(amount, Mathf.Max(0, maxEnergy - currentEnergy));
            gameData.energy = currentEnergy + addedEnergy;
            return addedEnergy;
        }

        gameData.SetResourceAmount(resourceType, gameData.GetResourceAmount(resourceType) + amount);
        return amount;
    }

    public bool CanAfford(IReadOnlyList<ResourceAmount> costs)
    {
        if (costs == null)
        {
            return true;
        }

        for (int i = 0; i < costs.Count; i++)
        {
            ResourceAmount cost = costs[i];

            if (cost == null || cost.amount <= 0)
            {
                continue;
            }

            if (GetAmount(cost.resourceType) < cost.amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool TrySpend(IReadOnlyList<ResourceAmount> costs)
    {
        if (!CanAfford(costs))
        {
            return false;
        }

        if (costs == null)
        {
            return true;
        }

        for (int i = 0; i < costs.Count; i++)
        {
            ResourceAmount cost = costs[i];

            if (cost == null || cost.amount <= 0)
            {
                continue;
            }

            gameData.SetResourceAmount(cost.resourceType, GetAmount(cost.resourceType) - cost.amount);
        }

        return true;
    }

    public int AddProducedOre(int amount, bool allowDuringShuttleLoading = false)
    {
        if (amount <= 0)
        {
            return 0;
        }

        if (!gameData.hasMiningPlatform)
        {
            return AddOreDirectlyToShuttle(amount);
        }

        if (platformSystem == null)
        {
            return 0;
        }

        return platformSystem.AddOre(amount, allowDuringShuttleLoading);
    }

    public void MineOre()
    {
        AddProducedOre(gameData.orePerClick, true);
    }

    public void AddPassiveOre()
    {
        AddProducedOre(gameData.orePerSecond);
    }

    public int GetPassiveOrePerSecond()
    {
        return Mathf.Max(0, gameData.orePerSecond);
    }

    public bool TryProduceMetal()
    {
        if (gameData.metalPerCraft <= 0)
        {
            return false;
        }

        List<ResourceAmount> productionCosts = GetMetalProductionCosts();

        if (!TrySpend(productionCosts))
        {
            return false;
        }

        AddResource(ResourceType.Metal, Mathf.Max(1, gameData.metalPerCraft));
        return true;
    }

    public List<ResourceAmount> GetMetalProductionCosts()
    {
        List<ResourceAmount> costs = new List<ResourceAmount>();

        if (gameData.metalOreCost > 0)
        {
            costs.Add(new ResourceAmount(ResourceType.Ore, gameData.metalOreCost));
        }

        if (gameData.metalEnergyCost > 0)
        {
            costs.Add(new ResourceAmount(ResourceType.Energy, gameData.metalEnergyCost));
        }

        return costs;
    }

    private int AddOreDirectlyToShuttle(int amount)
    {
        if (IsShuttleBusy())
        {
            return 0;
        }

        int freeCapacity = Mathf.Max(0, gameData.shuttleCapacity - gameData.shuttleOre);

        if (freeCapacity <= 0)
        {
            return 0;
        }

        int addedAmount = Mathf.Min(amount, freeCapacity);
        gameData.shuttleOre += addedAmount;
        gameData.totalOreEarned += addedAmount;
        return addedAmount;
    }

    private bool IsShuttleBusy()
    {
        return gameData.shuttleLoadingCooldownRemaining > 0f ||
               gameData.shuttleLoadingTargetOre > 0 ||
               gameData.shuttleLoadingOre > 0 ||
               gameData.shuttleSendCooldownRemaining > 0f ||
               gameData.shuttleDeliveringOre > 0;
    }
}
