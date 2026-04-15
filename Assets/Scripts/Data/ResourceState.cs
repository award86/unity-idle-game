using System;

[Serializable]
public class ResourceState
{
    public ResourceType resourceType;
    public int amount;

    public ResourceState()
    {
    }

    public ResourceState(ResourceType resourceType, int amount)
    {
        this.resourceType = resourceType;
        this.amount = Math.Max(0, amount);
    }
}
