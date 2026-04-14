using UnityEngine;

public class TemporaryBoostState
{
    public TemporaryBoostState(TemporaryBoostDefinition definition)
    {
        Definition = definition;
    }

    public TemporaryBoostDefinition Definition { get; }
    public bool IsAvailable { get; private set; }
    public bool IsActive { get; private set; }
    public bool ShouldShowInList => IsAvailable && !IsActive;
    public float TimeUntilAvailable { get; private set; }
    public int NextOreThreshold { get; private set; }
    public float ActiveRemainingTime { get; private set; }

    public float GetMultiplier()
    {
        return Mathf.Max(1f, Definition.multiplier);
    }

    public void SetAvailable(bool value)
    {
        IsAvailable = value;
    }

    public void SetTimeUntilAvailable(float value)
    {
        TimeUntilAvailable = Mathf.Max(0f, value);
    }

    public void SetNextOreThreshold(int value)
    {
        NextOreThreshold = Mathf.Max(0, value);
    }

    public void Activate(float durationSeconds)
    {
        IsActive = true;
        ActiveRemainingTime = Mathf.Max(0f, durationSeconds);
    }

    public void Deactivate()
    {
        IsActive = false;
        ActiveRemainingTime = 0f;
    }

    public void SetActiveRemainingTime(float value)
    {
        ActiveRemainingTime = Mathf.Max(0f, value);
    }
}
