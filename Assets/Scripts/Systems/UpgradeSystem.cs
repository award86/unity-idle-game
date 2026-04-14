using UnityEngine;

public class UpgradeSystem
{
    private readonly GameData gameData;

    public UpgradeSystem(GameData gameData)
    {
        this.gameData = gameData;
        ApplyUpgradeStats();
    }

    public int GetUpgradeCost()
    {
        return Mathf.CeilToInt(GameSettings.BaseUpgradeCost * Mathf.Pow(GameSettings.UpgradeCostMultiplier, gameData.upgradeLevel));
    }

    public bool UpgradeOrePerSecond()
    {
        int upgradeCost = GetUpgradeCost();

        if (gameData.ore < upgradeCost)
        {
            return false;
        }

        gameData.ore -= upgradeCost;
        gameData.upgradeLevel += 1;
        ApplyUpgradeStats();
        return true;
    }

    private void ApplyUpgradeStats()
    {
        gameData.orePerClick = GameSettings.BaseOrePerClick + (gameData.upgradeLevel * GameSettings.OrePerClickPerUpgradeLevel);
        gameData.orePerSecond = GameSettings.BaseOrePerSecond + (gameData.upgradeLevel * GameSettings.OrePerSecondPerUpgradeLevel);
    }
}
