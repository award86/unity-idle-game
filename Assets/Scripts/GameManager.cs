using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string OreKey = "ore";
    private const string OrePerClickKey = "orePerClick";
    private const string OrePerSecondKey = "orePerSecond";
    private const string LastSaveTimeKey = "lastSaveTime";
    private const string LegacyUpgradeLevelKey = "upgradeLevel";

    [SerializeField] private UIManager uiManager;
    [SerializeField] private UpgradeConfig upgradeConfig;

    private GameData gameData;
    private ResourceSystem resourceSystem;
    private UpgradeManager upgradeManager;
    private TimeSystem timeSystem;
    private float autoSaveTimer;
    private int pendingOfflineOre;

    private void Awake()
    {
        LoadGame();

        resourceSystem = new ResourceSystem(gameData);
        upgradeManager = new UpgradeManager(gameData, upgradeConfig);
        upgradeManager.LoadUpgradeLevels();
        upgradeManager.UpgradesChanged += HandleUpgradesChanged;
        timeSystem = new TimeSystem(resourceSystem);
        pendingOfflineOre = CalculateOfflineOre();
    }

    private void Start()
    {
        if (uiManager != null)
        {
            uiManager.InitializeUpgradeList(upgradeManager.UpgradeStates, HandleUpgradeBuyRequested);
        }

        RefreshUI();
        ShowOfflineRewardIfNeeded();
    }

    private void Update()
    {
        if (timeSystem.UpdateTimer(Time.deltaTime))
        {
            RefreshUI();
        }

        if (upgradeManager != null)
        {
            upgradeManager.Update(Time.deltaTime);
            UpdateBoostUI();
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
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideMenu();
        uiManager.HideResetConfirmation();
        uiManager.ToggleUpgradePanel();
    }

    public void OnMenuButtonClicked()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideUpgradePanel();
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
        PlayerPrefs.DeleteKey(LegacyUpgradeLevelKey);

        if (upgradeManager != null)
        {
            upgradeManager.SaveUpgradeLevels();
        }

        PlayerPrefs.SetString(LastSaveTimeKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        gameData = new GameData
        {
            ore = PlayerPrefs.GetInt(OreKey, GameSettings.StartOre),
            orePerClick = PlayerPrefs.GetInt(OrePerClickKey, GameSettings.StartOrePerClick),
            orePerSecond = PlayerPrefs.GetInt(OrePerSecondKey, GameSettings.StartOrePerSecond)
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
        }

        PlayerPrefs.DeleteKey(OreKey);
        PlayerPrefs.DeleteKey(OrePerClickKey);
        PlayerPrefs.DeleteKey(OrePerSecondKey);
        PlayerPrefs.DeleteKey(LegacyUpgradeLevelKey);
        PlayerPrefs.DeleteKey(LastSaveTimeKey);

        if (upgradeManager != null)
        {
            upgradeManager.ResetUpgrades();
        }

        if (uiManager != null)
        {
            uiManager.HideOfflineReward();
            uiManager.HideUpgradePanel();
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

        uiManager.UpdateUI(gameData);
        uiManager.RefreshUpgradeList(upgradeManager.UpgradeStates, gameData.ore);
        UpdateBoostUI();
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

    private void HandleUpgradeBuyRequested(UpgradeState state)
    {
        if (upgradeManager == null || !upgradeManager.TryBuyUpgrade(state))
        {
            return;
        }

        RefreshUI();
        SaveGame();
    }

    private void HandleUpgradesChanged()
    {
        RefreshUI();
    }

    private void UpdateBoostUI()
    {
        if (uiManager == null || upgradeManager == null)
        {
            return;
        }

        uiManager.UpdateBoostUI(
            upgradeManager.HasActiveBoost,
            upgradeManager.ActiveBoostName,
            upgradeManager.ActiveBoostMultiplier,
            upgradeManager.ActiveBoostRemainingTime);
    }

    private void OnDestroy()
    {
        if (upgradeManager != null)
        {
            upgradeManager.UpgradesChanged -= HandleUpgradesChanged;
        }
    }
}
