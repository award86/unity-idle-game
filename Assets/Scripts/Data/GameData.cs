using System;

[Serializable]
public class GameData
{
    public int ore = GameSettings.StartOre;
    public int shuttleOre = ShuttleConfig.DefaultStartOre;
    public int shuttleDeliveringOre = 0;
    public int shuttleCapacity = ShuttleConfig.DefaultCapacity;
    public float shuttleTravelTimeSeconds = ShuttleConfig.DefaultTravelTimeSeconds;
    public float shuttleSendCooldownRemaining = 0f;
    public bool shuttleAutoSendEnabled = false;
    public int orePerClick = GameSettings.StartOrePerClick;
    public int orePerSecond = GameSettings.StartOrePerSecond;
    public int totalOreEarned = 0;
}
