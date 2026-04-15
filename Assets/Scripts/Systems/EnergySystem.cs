using UnityEngine;

public class EnergySystem
{
    private readonly GameData gameData;
    private readonly ResourceSystem resourceSystem;

    public EnergySystem(GameData gameData, ResourceSystem resourceSystem)
    {
        this.gameData = gameData;
        this.resourceSystem = resourceSystem;
    }

    public bool Update(float deltaTime)
    {
        if (gameData.energy >= gameData.energyMax ||
            gameData.energyRegenAmount <= 0 ||
            gameData.energyRegenInterval <= 0f)
        {
            gameData.energyRegenTimer = 0f;
            return false;
        }

        bool energyChanged = false;
        gameData.energyRegenTimer += deltaTime;

        while (gameData.energyRegenTimer >= gameData.energyRegenInterval &&
               gameData.energy < gameData.energyMax)
        {
            gameData.energyRegenTimer -= gameData.energyRegenInterval;

            if (resourceSystem.AddResource(ResourceType.Energy, gameData.energyRegenAmount) <= 0)
            {
                gameData.energyRegenTimer = 0f;
                break;
            }

            energyChanged = true;
        }

        if (gameData.energy >= gameData.energyMax)
        {
            gameData.energyRegenTimer = 0f;
        }

        return energyChanged;
    }

    public EnergyOfflineProgress CalculateOfflineProgress(long offlineSeconds)
    {
        EnergyOfflineProgress progress = new EnergyOfflineProgress
        {
            energy = gameData.energy,
            regenTimer = gameData.energyRegenTimer
        };

        if (offlineSeconds <= 0 ||
            gameData.energy >= gameData.energyMax ||
            gameData.energyRegenAmount <= 0 ||
            gameData.energyRegenInterval <= 0f)
        {
            return progress;
        }

        float accumulatedTime = progress.regenTimer + offlineSeconds;

        while (accumulatedTime >= gameData.energyRegenInterval &&
               progress.energy < gameData.energyMax)
        {
            accumulatedTime -= gameData.energyRegenInterval;
            progress.energy = Mathf.Min(gameData.energyMax, progress.energy + gameData.energyRegenAmount);
        }

        progress.regenTimer = progress.energy >= gameData.energyMax ? 0f : accumulatedTime;
        return progress;
    }

    public struct EnergyOfflineProgress
    {
        public int energy;
        public float regenTimer;
    }
}
