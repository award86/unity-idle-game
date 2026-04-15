using UnityEngine;

public class PlatformSystem
{
    private readonly GameData gameData;

    public PlatformSystem(GameData gameData)
    {
        this.gameData = gameData;
    }

    public int AddOre(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int freeCapacity = GetFreeCapacity();

        if (freeCapacity <= 0)
        {
            return 0;
        }

        int addedAmount = Mathf.Min(amount, freeCapacity);
        gameData.shuttleOre += addedAmount;
        gameData.totalOreEarned += addedAmount;
        return addedAmount;
    }

    public int TakeOre(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int takenAmount = Mathf.Min(amount, GetStoredOre());
        gameData.shuttleOre -= takenAmount;
        return takenAmount;
    }

    public int GetStoredOre()
    {
        return Mathf.Max(0, gameData.shuttleOre);
    }

    public int GetFreeCapacity()
    {
        return Mathf.Max(0, gameData.platformCapacity - gameData.shuttleOre);
    }

    public bool HasOre()
    {
        return GetStoredOre() > 0;
    }

    public int GetAutoSendThreshold()
    {
        return Mathf.Max(1, Mathf.Min(gameData.platformCapacity, gameData.shuttleCapacity));
    }

    public bool HasEnoughOreForAutoSend()
    {
        return GetStoredOre() >= GetAutoSendThreshold();
    }
}
