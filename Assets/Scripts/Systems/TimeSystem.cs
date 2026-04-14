public class TimeSystem
{
    private readonly ResourceSystem resourceSystem;
    private float timer;

    public TimeSystem(ResourceSystem resourceSystem)
    {
        this.resourceSystem = resourceSystem;
    }

    public bool UpdateTimer(float deltaTime)
    {
        bool resourcesAdded = false;
        timer += deltaTime;

        while (timer >= 1f)
        {
            timer -= 1f;
            resourceSystem.AddPassiveIncome();
            resourcesAdded = true;
        }

        return resourcesAdded;
    }
}
