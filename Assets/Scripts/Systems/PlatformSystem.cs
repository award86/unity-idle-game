using UnityEngine;

public class PlatformSystem
{
    private readonly GameData gameData;

    public PlatformSystem(GameData gameData)
    {
        this.gameData = gameData;
    }

    public int AddOre(int amount, bool allowDuringShuttleLoading = false)
    {
        if (amount <= 0)
        {
            return 0;
        }

        if (!allowDuringShuttleLoading && IsLoadingToShuttle())
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
        return GetAutoSendThreshold(gameData.shuttleDockedOre, gameData.shuttleCapacity);
    }

    public int GetAutoSendThreshold(int currentDockedOre, int currentShuttleCapacity)
    {
        int remainingShuttleSpace = Mathf.Max(0, currentShuttleCapacity - currentDockedOre);

        if (remainingShuttleSpace <= 0)
        {
            return 0;
        }

        return Mathf.Max(1, Mathf.Min(gameData.platformCapacity, remainingShuttleSpace));
    }

    public bool HasEnoughOreForAutoSend()
    {
        int autoSendThreshold = GetAutoSendThreshold();
        return autoSendThreshold > 0 && GetStoredOre() >= autoSendThreshold;
    }

    public bool HasEnoughOreForAutoSend(int currentDockedOre, int currentShuttleCapacity)
    {
        int autoSendThreshold = GetAutoSendThreshold(currentDockedOre, currentShuttleCapacity);
        return autoSendThreshold > 0 && GetStoredOre() >= autoSendThreshold;
    }

    private bool IsLoadingToShuttle()
    {
        for (int i = 0; i < gameData.ActiveShuttleCount; i++)
        {
            ShuttleState shuttleState = gameData.GetShuttleState(i);

            if (shuttleState.loadingCooldownRemaining > 0f ||
                shuttleState.loadingTargetOre > 0)
            {
                return true;
            }
        }

        return false;
    }
}
