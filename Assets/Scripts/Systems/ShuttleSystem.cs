using UnityEngine;

public class ShuttleSystem
{
    private readonly GameData gameData;
    private readonly PlatformSystem platformSystem;

    public ShuttleSystem(GameData gameData, PlatformSystem platformSystem)
    {
        this.gameData = gameData;
        this.platformSystem = platformSystem;
    }

    public int SendToWarehouse()
    {
        return SendToWarehouse(0);
    }

    public int SendToWarehouse(int shuttleIndex)
    {
        return SendToWarehouseInternal(shuttleIndex, false);
    }

    public int AutoSendToWarehouse()
    {
        return AutoSendToWarehouse(0);
    }

    public int AutoSendToWarehouse(int shuttleIndex)
    {
        return SendToWarehouseInternal(shuttleIndex, true);
    }

    public bool CanSend(int shuttleIndex = 0)
    {
        if (!IsShuttleIndexUnlocked(shuttleIndex))
        {
            return false;
        }

        if (IsShuttleBusy(shuttleIndex))
        {
            return false;
        }

        if (!gameData.hasMiningPlatform)
        {
            return shuttleIndex == 0 && gameData.shuttleOre > 0;
        }

        ShuttleState shuttleState = GetShuttleState(shuttleIndex);
        return shuttleState.dockedOre > 0 || GetAvailablePlatformOreForLoading(shuttleIndex) > 0;
    }

    public bool IsReadyForAutoSend(int shuttleIndex = 0)
    {
        if (!IsShuttleIndexUnlocked(shuttleIndex) ||
            shuttleIndex >= gameData.ActiveAutoSendShuttleCount ||
            !CanSend(shuttleIndex))
        {
            return false;
        }

        if (!gameData.hasMiningPlatform)
        {
            return shuttleIndex == 0 && gameData.shuttleOre >= gameData.shuttleCapacity;
        }

        ShuttleState shuttleState = GetShuttleState(shuttleIndex);

        if (shuttleState.dockedOre >= gameData.shuttleCapacity)
        {
            return true;
        }

        int autoSendThreshold = platformSystem != null
            ? platformSystem.GetAutoSendThreshold(shuttleState.dockedOre, gameData.shuttleCapacity)
            : 0;

        return autoSendThreshold > 0 && GetAvailablePlatformOreForLoading(shuttleIndex) >= autoSendThreshold;
    }

    public bool Update(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return false;
        }

        bool hasChanges = false;

        for (int i = 0; i < gameData.ActiveShuttleCount; i++)
        {
            hasChanges = UpdateLoading(i, deltaTime) || hasChanges;
        }

        for (int i = 0; i < gameData.ActiveShuttleCount; i++)
        {
            hasChanges = UpdateTravel(i, deltaTime) || hasChanges;
        }

        return hasChanges;
    }

    public bool IsBusy()
    {
        for (int i = 0; i < gameData.ActiveShuttleCount; i++)
        {
            if (IsShuttleBusy(i))
            {
                return true;
            }
        }

        return false;
    }

    private int SendToWarehouseInternal(int shuttleIndex, bool requireFullCargo)
    {
        if (!CanSend(shuttleIndex))
        {
            return 0;
        }

        if (!gameData.hasMiningPlatform)
        {
            int directCargoAmount = Mathf.Min(Mathf.Max(0, gameData.shuttleOre), gameData.shuttleCapacity);

            if (directCargoAmount <= 0)
            {
                return 0;
            }

            gameData.shuttleOre -= directCargoAmount;
            StartTravelOrDeliver(GetShuttleState(0), directCargoAmount);
            return directCargoAmount;
        }

        ShuttleState shuttleState = GetShuttleState(shuttleIndex);
        int currentDockedOre = Mathf.Clamp(shuttleState.dockedOre, 0, gameData.shuttleCapacity);
        int remainingCapacity = Mathf.Max(0, gameData.shuttleCapacity - currentDockedOre);

        if (requireFullCargo && currentDockedOre >= gameData.shuttleCapacity)
        {
            shuttleState.dockedOre = 0;
            StartTravelOrDeliver(shuttleState, currentDockedOre);
            return currentDockedOre;
        }

        int transferAmount = Mathf.Min(GetAvailablePlatformOreForLoading(shuttleIndex), remainingCapacity);

        if (transferAmount <= 0)
        {
            if (requireFullCargo || currentDockedOre <= 0)
            {
                return 0;
            }

            shuttleState.dockedOre = 0;
            StartTravelOrDeliver(shuttleState, currentDockedOre);
            return currentDockedOre;
        }

        int targetCargo = currentDockedOre + transferAmount;
        bool shouldSendAfterLoading = !requireFullCargo || targetCargo >= gameData.shuttleCapacity;
        float loadingTimeSeconds = GetLoadingTimeSeconds();

        if (loadingTimeSeconds <= 0f)
        {
            int instantlyLoadedAmount = platformSystem != null
                ? platformSystem.TakeOre(transferAmount)
                : 0;

            if (instantlyLoadedAmount <= 0)
            {
                return 0;
            }

            shuttleState.dockedOre = Mathf.Clamp(currentDockedOre + instantlyLoadedAmount, 0, gameData.shuttleCapacity);

            if (shouldSendAfterLoading)
            {
                int cargoToSend = shuttleState.dockedOre;
                shuttleState.dockedOre = 0;
                StartTravelOrDeliver(shuttleState, cargoToSend);
                return cargoToSend;
            }

            return instantlyLoadedAmount;
        }

        shuttleState.loadingOre = 0;
        shuttleState.loadingTargetOre = transferAmount;
        shuttleState.loadingCooldownRemaining = loadingTimeSeconds;
        shuttleState.sendAfterLoading = shouldSendAfterLoading;
        return transferAmount;
    }

    private bool UpdateLoading(int shuttleIndex, float deltaTime)
    {
        ShuttleState shuttleState = GetShuttleState(shuttleIndex);

        if (shuttleState.loadingCooldownRemaining <= 0f || shuttleState.loadingTargetOre <= 0)
        {
            return false;
        }

        int previousLoadingOre = shuttleState.loadingOre;
        int previousPlatformOre = gameData.shuttleOre;
        int previousDisplayedSeconds = Mathf.CeilToInt(shuttleState.loadingCooldownRemaining);
        float loadingTimeSeconds = GetLoadingTimeSeconds();

        if (loadingTimeSeconds <= 0f)
        {
            CompleteLoading(shuttleState);
            return true;
        }

        float consumedDeltaTime = Mathf.Min(deltaTime, shuttleState.loadingCooldownRemaining);
        shuttleState.loadingCooldownRemaining = Mathf.Max(0f, shuttleState.loadingCooldownRemaining - consumedDeltaTime);

        float currentProgress = 1f - Mathf.Clamp01(shuttleState.loadingCooldownRemaining / loadingTimeSeconds);
        int desiredLoadedOre = shuttleState.loadingCooldownRemaining <= 0f
            ? shuttleState.loadingTargetOre
            : Mathf.Clamp(
                Mathf.FloorToInt(shuttleState.loadingTargetOre * currentProgress),
                0,
                shuttleState.loadingTargetOre);
        int deltaOre = Mathf.Max(0, desiredLoadedOre - shuttleState.loadingOre);

        if (deltaOre > 0 && platformSystem != null)
        {
            int transferredOre = platformSystem.TakeOre(deltaOre);
            shuttleState.loadingOre += transferredOre;
        }

        if (shuttleState.loadingCooldownRemaining <= 0f)
        {
            CompleteLoading(shuttleState);
        }

        return previousLoadingOre != shuttleState.loadingOre ||
               previousPlatformOre != gameData.shuttleOre ||
               previousDisplayedSeconds != Mathf.CeilToInt(shuttleState.loadingCooldownRemaining);
    }

    private void CompleteLoading(ShuttleState shuttleState)
    {
        int remainingOreToLoad = Mathf.Max(0, shuttleState.loadingTargetOre - shuttleState.loadingOre);

        if (remainingOreToLoad > 0 && platformSystem != null)
        {
            shuttleState.loadingOre += platformSystem.TakeOre(remainingOreToLoad);
        }

        shuttleState.dockedOre = Mathf.Clamp(
            shuttleState.dockedOre + shuttleState.loadingOre,
            0,
            gameData.shuttleCapacity);
        shuttleState.loadingOre = 0;
        shuttleState.loadingTargetOre = 0;
        shuttleState.loadingCooldownRemaining = 0f;

        if (shuttleState.sendAfterLoading)
        {
            int cargoToSend = shuttleState.dockedOre;
            shuttleState.dockedOre = 0;
            shuttleState.sendAfterLoading = false;
            StartTravelOrDeliver(shuttleState, cargoToSend);
            return;
        }

        shuttleState.sendAfterLoading = false;
    }

    private bool UpdateTravel(int shuttleIndex, float deltaTime)
    {
        ShuttleState shuttleState = GetShuttleState(shuttleIndex);

        if (shuttleState.sendCooldownRemaining <= 0f)
        {
            return false;
        }

        int previousDeliveringOre = shuttleState.deliveringOre;
        int previousWarehouseOre = gameData.ore;
        int previousDisplayedSeconds = Mathf.CeilToInt(shuttleState.sendCooldownRemaining);

        shuttleState.sendCooldownRemaining = Mathf.Max(0f, shuttleState.sendCooldownRemaining - deltaTime);

        if (shuttleState.sendCooldownRemaining <= 0f && shuttleState.deliveringOre > 0)
        {
            gameData.ore += shuttleState.deliveringOre;
            shuttleState.deliveringOre = 0;
        }

        return previousDeliveringOre != shuttleState.deliveringOre ||
               previousWarehouseOre != gameData.ore ||
               previousDisplayedSeconds != Mathf.CeilToInt(shuttleState.sendCooldownRemaining);
    }

    private void StartTravelOrDeliver(ShuttleState shuttleState, int cargoAmount)
    {
        if (cargoAmount <= 0)
        {
            shuttleState.deliveringOre = 0;
            shuttleState.sendCooldownRemaining = 0f;
            return;
        }

        float travelTimeSeconds = GetTravelTimeSeconds();

        if (travelTimeSeconds <= 0f)
        {
            gameData.ore += cargoAmount;
            shuttleState.deliveringOre = 0;
            shuttleState.sendCooldownRemaining = 0f;
            return;
        }

        shuttleState.deliveringOre = cargoAmount;
        shuttleState.sendCooldownRemaining = travelTimeSeconds;
    }

    private bool IsShuttleIndexUnlocked(int shuttleIndex)
    {
        return shuttleIndex >= 0 && shuttleIndex < gameData.ActiveShuttleCount;
    }

    private ShuttleState GetShuttleState(int shuttleIndex)
    {
        return gameData.GetShuttleState(shuttleIndex);
    }

    private bool IsShuttleBusy(int shuttleIndex)
    {
        ShuttleState shuttleState = GetShuttleState(shuttleIndex);
        return shuttleState.loadingCooldownRemaining > 0f ||
               shuttleState.loadingTargetOre > 0 ||
               shuttleState.loadingOre > 0 ||
               shuttleState.sendCooldownRemaining > 0f ||
               shuttleState.deliveringOre > 0;
    }

    private float GetLoadingTimeSeconds()
    {
        return Mathf.Max(0f, gameData.shuttleLoadingTimeSeconds);
    }

    private float GetTravelTimeSeconds()
    {
        return Mathf.Max(0f, gameData.shuttleTravelTimeSeconds);
    }

    private int GetAvailablePlatformOreForLoading(int shuttleIndex)
    {
        if (platformSystem == null)
        {
            return 0;
        }

        int storedPlatformOre = platformSystem.GetStoredOre();
        int reservedOre = 0;

        for (int i = 0; i < gameData.ActiveShuttleCount; i++)
        {
            if (i == shuttleIndex)
            {
                continue;
            }

            ShuttleState shuttleState = GetShuttleState(i);
            reservedOre += Mathf.Max(0, shuttleState.loadingTargetOre - shuttleState.loadingOre);
        }

        return Mathf.Max(0, storedPlatformOre - reservedOre);
    }
}
