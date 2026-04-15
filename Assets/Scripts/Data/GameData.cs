using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public List<ResourceState> resources = new List<ResourceState>();
    public bool hasMiningPlatform = false;
    public int shuttleOre = ShuttleConfig.DefaultStartOre;
    public int shuttleLoadingOre = 0;
    public int shuttleLoadingTargetOre = 0;
    public int shuttleDeliveringOre = 0;
    public int platformCapacity = ShuttleConfig.DefaultPlatformCapacity;
    public int shuttleCapacity = ShuttleConfig.DefaultCapacity;
    public float shuttleLoadingTimeSeconds = ShuttleConfig.DefaultLoadingTimeSeconds;
    public float shuttleLoadingCooldownRemaining = 0f;
    public float shuttleTravelTimeSeconds = ShuttleConfig.DefaultTravelTimeSeconds;
    public float shuttleSendCooldownRemaining = 0f;
    public bool shuttleAutoSendEnabled = false;
    public int orePerClick = ShuttleConfig.DefaultStartOrePerClick;
    public int orePerSecond = ShuttleConfig.DefaultStartOrePerSecond;
    public int energyMax = ShuttleConfig.DefaultStartEnergyMax;
    public int energyRegenAmount = ShuttleConfig.DefaultStartEnergyRegenAmount;
    public float energyRegenInterval = ShuttleConfig.DefaultStartEnergyRegenInterval;
    public float energyRegenTimer = 0f;
    public int metalPerCraft = ShuttleConfig.DefaultMetalPerCraft;
    public int metalOreCost = ShuttleConfig.DefaultMetalOreCost;
    public int metalEnergyCost = ShuttleConfig.DefaultMetalEnergyCost;
    public int totalOreEarned = 0;

    public GameData()
    {
        EnsureDefaultResources();
    }

    public int ore
    {
        get => GetResourceAmount(ResourceType.Ore);
        set => SetResourceAmount(ResourceType.Ore, value);
    }

    public int energy
    {
        get => GetResourceAmount(ResourceType.Energy);
        set => SetResourceAmount(ResourceType.Energy, value);
    }

    public int metal
    {
        get => GetResourceAmount(ResourceType.Metal);
        set => SetResourceAmount(ResourceType.Metal, value);
    }

    public int crystal
    {
        get => GetResourceAmount(ResourceType.Crystal);
        set => SetResourceAmount(ResourceType.Crystal, value);
    }

    public void EnsureDefaultResources()
    {
        EnsureResourceState(ResourceType.Ore, ShuttleConfig.DefaultStartOre);
        EnsureResourceState(ResourceType.Energy, ShuttleConfig.DefaultStartEnergy);
        EnsureResourceState(ResourceType.Metal, ShuttleConfig.DefaultStartMetal);
        EnsureResourceState(ResourceType.Crystal, ShuttleConfig.DefaultStartCrystal);
    }

    public int GetResourceAmount(ResourceType resourceType)
    {
        EnsureDefaultResources();
        ResourceState state = FindResourceState(resourceType);
        return state != null ? Math.Max(0, state.amount) : 0;
    }

    public void SetResourceAmount(ResourceType resourceType, int amount)
    {
        EnsureDefaultResources();
        ResourceState state = EnsureResourceState(resourceType, 0);
        state.amount = Math.Max(0, amount);
    }

    public GameData Clone()
    {
        GameData clone = new GameData();
        clone.CopyFrom(this);
        return clone;
    }

    public void CopyFrom(GameData source)
    {
        if (source == null)
        {
            return;
        }

        resources = new List<ResourceState>();

        for (int i = 0; i < source.resources.Count; i++)
        {
            ResourceState sourceState = source.resources[i];
            resources.Add(new ResourceState(sourceState.resourceType, sourceState.amount));
        }

        hasMiningPlatform = source.hasMiningPlatform;
        shuttleOre = source.shuttleOre;
        shuttleLoadingOre = source.shuttleLoadingOre;
        shuttleLoadingTargetOre = source.shuttleLoadingTargetOre;
        shuttleDeliveringOre = source.shuttleDeliveringOre;
        platformCapacity = source.platformCapacity;
        shuttleCapacity = source.shuttleCapacity;
        shuttleLoadingTimeSeconds = source.shuttleLoadingTimeSeconds;
        shuttleLoadingCooldownRemaining = source.shuttleLoadingCooldownRemaining;
        shuttleTravelTimeSeconds = source.shuttleTravelTimeSeconds;
        shuttleSendCooldownRemaining = source.shuttleSendCooldownRemaining;
        shuttleAutoSendEnabled = source.shuttleAutoSendEnabled;
        orePerClick = source.orePerClick;
        orePerSecond = source.orePerSecond;
        energyMax = source.energyMax;
        energyRegenAmount = source.energyRegenAmount;
        energyRegenInterval = source.energyRegenInterval;
        energyRegenTimer = source.energyRegenTimer;
        metalPerCraft = source.metalPerCraft;
        metalOreCost = source.metalOreCost;
        metalEnergyCost = source.metalEnergyCost;
        totalOreEarned = source.totalOreEarned;

        EnsureDefaultResources();
    }

    private ResourceState FindResourceState(ResourceType resourceType)
    {
        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].resourceType == resourceType)
            {
                return resources[i];
            }
        }

        return null;
    }

    private ResourceState EnsureResourceState(ResourceType resourceType, int defaultAmount)
    {
        ResourceState existingState = FindResourceState(resourceType);

        if (existingState != null)
        {
            return existingState;
        }

        ResourceState newState = new ResourceState(resourceType, defaultAmount);
        resources.Add(newState);
        return newState;
    }
}
