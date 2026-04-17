using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public const int MaxShuttles = 3;

    public List<ResourceState> resources = new List<ResourceState>();
    public List<ShuttleState> shuttleStates = new List<ShuttleState>();
    public bool hasMiningPlatform = false;
    public int shuttleOre = ShuttleConfig.DefaultStartOre;
    public int shuttleCount = 1;
    public int shuttleAutoSendCount = 0;
    public int platformCapacity = ShuttleConfig.DefaultPlatformCapacity;
    public int shuttleCapacity = ShuttleConfig.DefaultCapacity;
    public float shuttleLoadingTimeSeconds = ShuttleConfig.DefaultLoadingTimeSeconds;
    public float shuttleTravelTimeSeconds = ShuttleConfig.DefaultTravelTimeSeconds;
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
        EnsureDefaultShuttles();
    }

    public int shuttleDockedOre
    {
        get => GetShuttleState(0).dockedOre;
        set => GetShuttleState(0).dockedOre = Math.Max(0, value);
    }

    public int shuttleLoadingOre
    {
        get => GetShuttleState(0).loadingOre;
        set => GetShuttleState(0).loadingOre = Math.Max(0, value);
    }

    public int shuttleLoadingTargetOre
    {
        get => GetShuttleState(0).loadingTargetOre;
        set => GetShuttleState(0).loadingTargetOre = Math.Max(shuttleLoadingOre, value);
    }

    public bool shuttleSendAfterLoading
    {
        get => GetShuttleState(0).sendAfterLoading;
        set => GetShuttleState(0).sendAfterLoading = value;
    }

    public int shuttleDeliveringOre
    {
        get => GetShuttleState(0).deliveringOre;
        set => GetShuttleState(0).deliveringOre = Math.Max(0, value);
    }

    public float shuttleLoadingCooldownRemaining
    {
        get => GetShuttleState(0).loadingCooldownRemaining;
        set => GetShuttleState(0).loadingCooldownRemaining = Math.Max(0f, value);
    }

    public float shuttleSendCooldownRemaining
    {
        get => GetShuttleState(0).sendCooldownRemaining;
        set => GetShuttleState(0).sendCooldownRemaining = Math.Max(0f, value);
    }

    public bool shuttleAutoSendEnabled
    {
        get => ActiveAutoSendShuttleCount > 0;
        set => shuttleAutoSendCount = value ? Math.Max(1, shuttleAutoSendCount) : 0;
    }

    public int ActiveShuttleCount => ClampShuttleCount(shuttleCount);
    public int ActiveAutoSendShuttleCount => Math.Min(ActiveShuttleCount, Math.Max(0, shuttleAutoSendCount));

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

    public void EnsureDefaultShuttles()
    {
        while (shuttleStates.Count < MaxShuttles)
        {
            shuttleStates.Add(new ShuttleState());
        }
    }

    public ShuttleState GetShuttleState(int index)
    {
        EnsureDefaultShuttles();

        if (index < 0)
        {
            index = 0;
        }

        while (shuttleStates.Count <= index)
        {
            shuttleStates.Add(new ShuttleState());
        }

        return shuttleStates[index];
    }

    public void ResetUnusedShuttles()
    {
        EnsureDefaultShuttles();

        for (int i = ActiveShuttleCount; i < shuttleStates.Count; i++)
        {
            shuttleStates[i].Reset();
        }
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
        shuttleStates = new List<ShuttleState>();

        for (int i = 0; i < source.resources.Count; i++)
        {
            ResourceState sourceState = source.resources[i];
            resources.Add(new ResourceState(sourceState.resourceType, sourceState.amount));
        }

        for (int i = 0; i < source.shuttleStates.Count; i++)
        {
            shuttleStates.Add(source.shuttleStates[i].Clone());
        }

        hasMiningPlatform = source.hasMiningPlatform;
        shuttleOre = source.shuttleOre;
        shuttleCount = source.shuttleCount;
        shuttleAutoSendCount = source.shuttleAutoSendCount;
        platformCapacity = source.platformCapacity;
        shuttleCapacity = source.shuttleCapacity;
        shuttleLoadingTimeSeconds = source.shuttleLoadingTimeSeconds;
        shuttleTravelTimeSeconds = source.shuttleTravelTimeSeconds;
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
        EnsureDefaultShuttles();
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

    private int ClampShuttleCount(int count)
    {
        return Math.Max(1, Math.Min(MaxShuttles, count));
    }
}
