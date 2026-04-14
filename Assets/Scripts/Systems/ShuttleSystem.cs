using UnityEngine;

public class ShuttleSystem
{
    private readonly GameData gameData;

    public ShuttleSystem(GameData gameData)
    {
        this.gameData = gameData;
    }

    public int AddToShuttle(int amount)
    {
        if (amount <= 0 || gameData.shuttleSendCooldownRemaining > 0f)
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

    public int SendToWarehouse()
    {
        if (!CanSend())
        {
            return 0;
        }

        int sentAmount = gameData.shuttleOre;
        gameData.shuttleOre = 0;
        gameData.ore += sentAmount;
        gameData.shuttleSendCooldownRemaining = GetTravelTimeSeconds();
        return sentAmount;
    }

    public int GetFreeCapacity()
    {
        return Mathf.Max(0, gameData.shuttleCapacity - gameData.shuttleOre);
    }

    public bool CanSend()
    {
        return gameData.shuttleOre > 0 && gameData.shuttleSendCooldownRemaining <= 0f;
    }

    public bool IsFull()
    {
        return gameData.shuttleCapacity > 0 && gameData.shuttleOre >= gameData.shuttleCapacity;
    }

    public bool UpdateCooldown(float deltaTime)
    {
        if (gameData.shuttleSendCooldownRemaining <= 0f)
        {
            return false;
        }

        int previousDisplayedSeconds = Mathf.CeilToInt(gameData.shuttleSendCooldownRemaining);
        gameData.shuttleSendCooldownRemaining = Mathf.Max(0f, gameData.shuttleSendCooldownRemaining - deltaTime);
        int currentDisplayedSeconds = Mathf.CeilToInt(gameData.shuttleSendCooldownRemaining);
        return currentDisplayedSeconds != previousDisplayedSeconds;
    }

    private float GetTravelTimeSeconds()
    {
        return Mathf.Max(0f, gameData.shuttleTravelTimeSeconds);
    }
}
