using UnityEngine;

public static class NumberFormatter
{
    public static string FormatInt(int value)
    {
        if (value >= GameSettings.NumberFormatB)
        {
            return (value / (float)GameSettings.NumberFormatB).ToString("0.#") + "B";
        }

        if (value >= GameSettings.NumberFormatM)
        {
            return (value / (float)GameSettings.NumberFormatM).ToString("0.#") + "M";
        }

        if (value >= GameSettings.NumberFormatK)
        {
            return (value / (float)GameSettings.NumberFormatK).ToString("0.#") + "K";
        }

        return value.ToString();
    }

    public static string FormatFloat(float value)
    {
        if (Mathf.Approximately(value, Mathf.Round(value)))
        {
            return Mathf.RoundToInt(value).ToString();
        }

        return value.ToString("0.##");
    }
}
