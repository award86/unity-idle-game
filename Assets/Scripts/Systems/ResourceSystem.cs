public class ResourceSystem
{
    private readonly GameData gameData;

    public ResourceSystem(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void AddOre(int amount)
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
