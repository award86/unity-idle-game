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

        int sentAmount = platformSystem != null
            ? platformSystem.TakeOre(gameData.shuttleCapacity)
            : 0;

        if (sentAmount <= 0)
        {
            return 0;
        }

        float travelTimeSeconds = GetTravelTimeSeconds();

        if (travelTimeSeconds <= 0f)
        {
            gameData.ore += sentAmount;
            gameData.shuttleDeliveringOre = 0;
            gameData.shuttleSendCooldownRemaining = 0f;
            return sentAmount;
        }

        gameData.shuttleDeliveringOre = sentAmount;
        gameData.shuttleSendCooldownRemaining = travelTimeSeconds;
        return sentAmount;
    }

    public bool CanSend()
    {
        return platformSystem != null &&
               platformSystem.HasOre() &&
               gameData.shuttleDeliveringOre <= 0 &&
               gameData.shuttleSendCooldownRemaining <= 0f;
    }

    public bool IsReadyForAutoSend()
    {
        return CanSend() &&
               platformSystem != null &&
               platformSystem.HasEnoughOreForAutoSend();
    }

    public bool UpdateCooldown(float deltaTime)
    {
        if (gameData.shuttleSendCooldownRemaining <= 0f)
        {
            return false;
        }

        int previousDisplayedSeconds = Mathf.CeilToInt(gameData.shuttleSendCooldownRemaining);
        gameData.shuttleSendCooldownRemaining = Mathf.Max(0f, gameData.shuttleSendCooldownRemaining - deltaTime);
        bool deliveryCompleted = gameData.shuttleSendCooldownRemaining <= 0f && gameData.shuttleDeliveringOre > 0;

        if (deliveryCompleted)
        {
            gameData.ore += gameData.shuttleDeliveringOre;
            gameData.shuttleDeliveringOre = 0;
        }

        int currentDisplayedSeconds = Mathf.CeilToInt(gameData.shuttleSendCooldownRemaining);
        return currentDisplayedSeconds != previousDisplayedSeconds || deliveryCompleted;
    }

    private float GetTravelTimeSeconds()
    {
        return Mathf.Max(0f, gameData.shuttleTravelTimeSeconds);
    }
}
