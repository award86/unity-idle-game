using System;

[Serializable]
public class ResourceAmount
{
    public ResourceType resourceType = ResourceType.Ore;
    public int amount = 0;

    public ResourceAmount()
    {
    }

    public ResourceAmount(ResourceType resourceType, int amount)
    {
        this.resourceType = resourceType;
        this.amount = Math.Max(0, amount);
    }
}
