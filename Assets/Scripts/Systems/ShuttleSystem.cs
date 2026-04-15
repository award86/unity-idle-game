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
        if (!CanSend())
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
            StartTravelOrDeliver(directCargoAmount);
            return directCargoAmount;
        }

        int sentAmount = platformSystem != null
            ? Mathf.Min(platformSystem.GetStoredOre(), gameData.shuttleCapacity)
            : 0;

        if (sentAmount <= 0)
        {
            return 0;
        }

        float loadingTimeSeconds = GetLoadingTimeSeconds();

        if (loadingTimeSeconds <= 0f)
        {
            int instantlyLoadedAmount = platformSystem != null
                ? platformSystem.TakeOre(gameData.shuttleCapacity)
                : 0;

            if (instantlyLoadedAmount <= 0)
            {
                return 0;
            }

            StartTravelOrDeliver(instantlyLoadedAmount);
            return instantlyLoadedAmount;
        }

        gameData.shuttleLoadingOre = 0;
        gameData.shuttleLoadingTargetOre = sentAmount;
        gameData.shuttleLoadingCooldownRemaining = loadingTimeSeconds;
        return sentAmount;
    }

    public bool CanSend()
    {
        if (IsBusy())
        {
            return false;
        }

        if (!gameData.hasMiningPlatform)
        {
            return gameData.shuttleOre > 0;
        }

        return platformSystem != null && platformSystem.HasOre();
    }

    public bool IsReadyForAutoSend()
    {
        return CanSend() &&
               platformSystem != null &&
               platformSystem.HasEnoughOreForAutoSend();
    }

    public bool Update(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return false;
        }

        int previousPlatformOre = gameData.shuttleOre;
        int previousLoadingOre = gameData.shuttleLoadingOre;
        int previousDeliveringOre = gameData.shuttleDeliveringOre;
        int previousLoadingDisplayedSeconds = Mathf.CeilToInt(gameData.shuttleLoadingCooldownRemaining);
        int previousTravelDisplayedSeconds = Mathf.CeilToInt(gameData.shuttleSendCooldownRemaining);

        float remainingDeltaTime = deltaTime;
        remainingDeltaTime = UpdateLoading(remainingDeltaTime);
        UpdateTravel(remainingDeltaTime);

        int currentLoadingDisplayedSeconds = Mathf.CeilToInt(gameData.shuttleLoadingCooldownRemaining);
        int currentTravelDisplayedSeconds = Mathf.CeilToInt(gameData.shuttleSendCooldownRemaining);

        return previousPlatformOre != gameData.shuttleOre ||
               previousLoadingOre != gameData.shuttleLoadingOre ||
               previousDeliveringOre != gameData.shuttleDeliveringOre ||
               previousLoadingDisplayedSeconds != currentLoadingDisplayedSeconds ||
               previousTravelDisplayedSeconds != currentTravelDisplayedSeconds;
    }

    public bool IsBusy()
    {
        return gameData.shuttleLoadingCooldownRemaining > 0f ||
               gameData.shuttleSendCooldownRemaining > 0f ||
               gameData.shuttleLoadingTargetOre > 0 ||
               gameData.shuttleLoadingOre > 0 ||
               gameData.shuttleDeliveringOre > 0;
    }

    private float UpdateLoading(float deltaTime)
    {
        if (gameData.shuttleLoadingCooldownRemaining <= 0f || gameData.shuttleLoadingTargetOre <= 0)
        {
            return deltaTime;
        }

        float loadingTimeSeconds = GetLoadingTimeSeconds();

        if (loadingTimeSeconds <= 0f)
        {
            CompleteLoading();
            return deltaTime;
        }

        float consumedDeltaTime = Mathf.Min(deltaTime, gameData.shuttleLoadingCooldownRemaining);
        gameData.shuttleLoadingCooldownRemaining = Mathf.Max(0f, gameData.shuttleLoadingCooldownRemaining - consumedDeltaTime);

        float currentProgress = 1f - Mathf.Clamp01(gameData.shuttleLoadingCooldownRemaining / loadingTimeSeconds);
        int desiredLoadedOre = gameData.shuttleLoadingCooldownRemaining <= 0f
            ? gameData.shuttleLoadingTargetOre
            : Mathf.Clamp(
                Mathf.FloorToInt(gameData.shuttleLoadingTargetOre * currentProgress),
                0,
                gameData.shuttleLoadingTargetOre);
        int deltaOre = Mathf.Max(0, desiredLoadedOre - gameData.shuttleLoadingOre);

        if (deltaOre > 0 && platformSystem != null)
        {
            int transferredOre = platformSystem.TakeOre(deltaOre);
            gameData.shuttleLoadingOre += transferredOre;
        }

        if (gameData.shuttleLoadingCooldownRemaining <= 0f)
        {
            CompleteLoading();
        }

        return Mathf.Max(0f, deltaTime - consumedDeltaTime);
    }

    private void CompleteLoading()
    {
        int remainingOreToLoad = Mathf.Max(0, gameData.shuttleLoadingTargetOre - gameData.shuttleLoadingOre);

        if (remainingOreToLoad > 0 && platformSystem != null)
        {
            gameData.shuttleLoadingOre += platformSystem.TakeOre(remainingOreToLoad);
        }

        int loadedOre = Mathf.Max(gameData.shuttleLoadingOre, gameData.shuttleLoadingTargetOre);
        gameData.shuttleLoadingOre = 0;
        gameData.shuttleLoadingTargetOre = 0;
        gameData.shuttleLoadingCooldownRemaining = 0f;

        StartTravelOrDeliver(loadedOre);
    }

    private void UpdateTravel(float deltaTime)
    {
        if (gameData.shuttleSendCooldownRemaining <= 0f)
        {
            return;
        }

        gameData.shuttleSendCooldownRemaining = Mathf.Max(0f, gameData.shuttleSendCooldownRemaining - deltaTime);

        if (gameData.shuttleSendCooldownRemaining <= 0f && gameData.shuttleDeliveringOre > 0)
        {
            gameData.ore += gameData.shuttleDeliveringOre;
            gameData.shuttleDeliveringOre = 0;
        }
    }

    private void StartTravelOrDeliver(int cargoAmount)
    {
        if (cargoAmount <= 0)
        {
            gameData.shuttleDeliveringOre = 0;
            gameData.shuttleSendCooldownRemaining = 0f;
            return;
        }

        float travelTimeSeconds = GetTravelTimeSeconds();

        if (travelTimeSeconds <= 0f)
        {
            gameData.ore += cargoAmount;
            gameData.shuttleDeliveringOre = 0;
            gameData.shuttleSendCooldownRemaining = 0f;
            return;
        }

        gameData.shuttleDeliveringOre = cargoAmount;
        gameData.shuttleSendCooldownRemaining = travelTimeSeconds;
    }

    private float GetLoadingTimeSeconds()
    {
        return Mathf.Max(0f, gameData.shuttleLoadingTimeSeconds);
    }

    private float GetTravelTimeSeconds()
    {
        return Mathf.Max(0f, gameData.shuttleTravelTimeSeconds);
    }
}
