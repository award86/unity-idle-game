using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string OreKey = "ore";
    private const string OrePerClickKey = "orePerClick";
    private const string OrePerSecondKey = "orePerSecond";
    private const string UpgradeLevelKey = "upgradeLevel";
    private const string LastSaveTimeKey = "lastSaveTime";

    [SerializeField] private UIManager uiManager;

    private GameData gameData;
    private ResourceSystem resourceSystem;
    private UpgradeSystem upgradeSystem;
    private TimeSystem timeSystem;
    private float autoSaveTimer;
    private int pendingOfflineOre;

    private void Awake()
    {
        LoadGame();

        resourceSystem = new ResourceSystem(gameData);
        upgradeSystem = new UpgradeSystem(gameData);
        timeSystem = new TimeSystem(resourceSystem);
        pendingOfflineOre = CalculateOfflineOre();

        RefreshUI();
        ShowOfflineRewardIfNeeded();
    }

    private void Update()
    {
        if (timeSystem.UpdateTimer(Time.deltaTime))
        {
            RefreshUI();
        }

        autoSaveTimer += Time.deltaTime;

        if (autoSaveTimer >= GameSettings.AutoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveGame();
        }
    }

    public void OnMineButtonClicked()
    {
        resourceSystem.Mine();
        RefreshUI();
    }

    public void OnUpgradeButtonClicked()
    {
        if (!upgradeSystem.UpgradeOrePerSecond())
        {
            return;
        }

        RefreshUI();
        SaveGame();
    }

    public void OnMenuButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideResetConfirmation();
        uiManager.ToggleMenu();
    }

    public void OnResetProgressButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideMenu();
        uiManager.ShowResetConfirmation();
    }

    public void OnConfirmResetButtonClicked()
    {
        ResetGame();
        SaveGame();
        RefreshUI();

        if (uiManager == null)
        {
            return;
        }

        uiManager.HideResetConfirmation();
        uiManager.HideMenu();
    }

    public void OnCancelResetButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideResetConfirmation();
    }

    public void OnClaimOfflineRewardButtonClicked()
    {
        if (pendingOfflineOre > 0)
        {
            resourceSystem.AddOre(pendingOfflineOre);
            pendingOfflineOre = 0;
        }

        if (uiManager != null)
        {
            uiManager.HideOfflineReward();
        }

        RefreshUI();
        SaveGame();
    }

    public void OnClaimOfflineRewardX2ButtonClicked()
    {
        // TODO: Show rewarded ad and give x2 offline reward after ad is completed.
        Debug.Log("Rewarded ad is not implemented yet.");
    }

    public void SaveGame()
    {
        if (gameData == null)
        {
            return;
        }

        PlayerPrefs.SetInt(OreKey, gameData.ore);
        PlayerPrefs.SetInt(OrePerClickKey, gameData.orePerClick);
        PlayerPrefs.SetInt(OrePerSecondKey, gameData.orePerSecond);
        PlayerPrefs.SetInt(UpgradeLevelKey, gameData.upgradeLevel);
        PlayerPrefs.SetString(LastSaveTimeKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        bool hasUpgradeLevel = PlayerPrefs.HasKey(UpgradeLevelKey);
        int savedOrePerSecond = Mathf.Max(0, PlayerPrefs.GetInt(OrePerSecondKey, 0));
        int savedUpgradeLevel = hasUpgradeLevel
            ? Mathf.Max(0, PlayerPrefs.GetInt(UpgradeLevelKey, 0))
            : savedOrePerSecond;

        gameData = new GameData
        {
            ore = PlayerPrefs.GetInt(OreKey, GameSettings.StartOre),
            orePerClick = hasUpgradeLevel
                ? Mathf.Max(GameSettings.StartOrePerClick, PlayerPrefs.GetInt(OrePerClickKey, GameSettings.StartOrePerClick))
                : GameSettings.BaseOrePerClick + (savedUpgradeLevel * GameSettings.OrePerClickPerUpgradeLevel),
            orePerSecond = hasUpgradeLevel
                ? savedOrePerSecond
                : GameSettings.BaseOrePerSecond + (savedUpgradeLevel * GameSettings.OrePerSecondPerUpgradeLevel),
            upgradeLevel = savedUpgradeLevel
        };
    }

    public void ResetGame()
    {
        pendingOfflineOre = 0;

        if (gameData == null)
        {
            gameData = new GameData();
        }
        else
        {
            gameData.ore = GameSettings.StartOre;
            gameData.orePerClick = GameSettings.StartOrePerClick;
            gameData.orePerSecond = GameSettings.StartOrePerSecond;
            gameData.upgradeLevel = GameSettings.StartUpgradeLevel;
        }

        PlayerPrefs.DeleteKey(OreKey);
        PlayerPrefs.DeleteKey(OrePerClickKey);
        PlayerPrefs.DeleteKey(OrePerSecondKey);
        PlayerPrefs.DeleteKey(UpgradeLevelKey);
        PlayerPrefs.DeleteKey(LastSaveTimeKey);

        if (uiManager != null)
        {
            uiManager.HideOfflineReward();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void RefreshUI()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.UpdateUI(gameData, upgradeSystem.GetUpgradeCost());
    }

    private int CalculateOfflineOre()
    {
        string savedTime = PlayerPrefs.GetString(LastSaveTimeKey, string.Empty);

        if (string.IsNullOrEmpty(savedTime))
        {
            return 0;
        }

        if (!long.TryParse(savedTime, out long lastSaveTime))
        {
            return 0;
        }

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long offlineSeconds = currentTime - lastSaveTime;

        if (offlineSeconds <= 0 || gameData.orePerSecond <= 0)
        {
            return 0;
        }

        offlineSeconds = Math.Min(offlineSeconds, GameSettings.MaxOfflineSeconds);
        long earnedOre = offlineSeconds * gameData.orePerSecond;
        long clampedOre = Math.Min(earnedOre, int.MaxValue);
        return (int)clampedOre;
    }

    private void ShowOfflineRewardIfNeeded()
    {
        if (pendingOfflineOre <= 0 || uiManager == null)
        {
            return;
        }

        uiManager.ShowOfflineReward(pendingOfflineOre);
    }
}
