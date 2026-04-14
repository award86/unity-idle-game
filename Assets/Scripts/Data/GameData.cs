using System;

[Serializable]
public class GameData
{
    public int ore = GameSettings.StartOre;
    public int orePerClick = GameSettings.StartOrePerClick;
    public int orePerSecond = GameSettings.StartOrePerSecond;
    public int totalOreEarned = 0;
}
