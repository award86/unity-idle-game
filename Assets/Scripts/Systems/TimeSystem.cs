public class TimeSystem
{
    private readonly ResourceSystem resourceSystem;
    private float oreTimer;

    public TimeSystem(ResourceSystem resourceSystem)
    {
        this.resourceSystem = resourceSystem;
    }

    public bool UpdateTimer(float deltaTime)
    {
        bool resourcesAdded = false;
        oreTimer += deltaTime;

        while (oreTimer >= 1f)
        {
            oreTimer -= 1f;
            resourceSystem.AddPassiveOre();
            resourcesAdded = true;
        }

        return resourcesAdded;
    }
}
