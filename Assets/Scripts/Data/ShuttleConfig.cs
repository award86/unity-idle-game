using UnityEngine;

[CreateAssetMenu(fileName = "ShuttleConfig", menuName = "Idle Space/Shuttle Config")]
public class ShuttleConfig : ScriptableObject
{
    public const int DefaultStartOre = 0;
    public const int DefaultCapacity = 100;
    public const float DefaultTravelTimeSeconds = 60f;

    [SerializeField] private int startOre = DefaultStartOre;
    [SerializeField] private int capacity = DefaultCapacity;
    [SerializeField] private float travelTimeSeconds = DefaultTravelTimeSeconds;

    public int StartOre => Mathf.Max(0, startOre);
    public int Capacity => Mathf.Max(1, capacity);
    public float TravelTimeSeconds => Mathf.Max(0f, travelTimeSeconds);
}
