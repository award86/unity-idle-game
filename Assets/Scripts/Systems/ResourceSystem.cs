public class ResourceSystem
{
    private readonly GameData gameData;
    private readonly ShuttleSystem shuttleSystem;

    public ResourceSystem(GameData gameData, ShuttleSystem shuttleSystem)
    {
        this.gameData = gameData;
        this.shuttleSystem = shuttleSystem;
    }

    public int AddOre(int amount)
    {
        if (shuttleSystem == null)
        {
            return 0;
        }

        return shuttleSystem.AddToShuttle(amount);
    }

    public void AddOreToWarehouse(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        gameData.ore += amount;
        gameData.totalOreEarned += amount;
    }

    public void Mine()
    {
        AddOre(gameData.orePerClick);
    }

    public void AddPassiveIncome()
    {
        AddOre(gameData.orePerSecond);
    }
}
