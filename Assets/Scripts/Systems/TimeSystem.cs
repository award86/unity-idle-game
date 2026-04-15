public class TimeSystem
{
    private readonly ResourceSystem resourceSystem;
    private float passiveOreBuffer;

    public TimeSystem(ResourceSystem resourceSystem)
    {
        this.resourceSystem = resourceSystem;
    }

    public bool UpdateTimer(float deltaTime)
    {
        if (resourceSystem == null || deltaTime <= 0f)
        {
            return false;
        }

        int orePerSecond = resourceSystem.GetPassiveOrePerSecond();

        if (orePerSecond <= 0)
        {
            passiveOreBuffer = 0f;
            return false;
        }

        bool resourcesAdded = false;
        passiveOreBuffer += orePerSecond * deltaTime;

        while (passiveOreBuffer >= 1f)
        {
            int addedOre = resourceSystem.AddProducedOre(1);

            if (addedOre <= 0)
            {
                passiveOreBuffer = 0f;
                break;
            }

            passiveOreBuffer -= 1f;
            resourcesAdded = true;
        }

        return resourcesAdded;
    }
}
