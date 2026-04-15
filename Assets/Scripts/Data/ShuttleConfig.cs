using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Idle Space/Game Config")]
public class ShuttleConfig : ScriptableObject
{
    public const int DefaultStartOre = 0;
    public const int DefaultStartEnergy = 10;
    public const int DefaultStartMetal = 0;
    public const int DefaultStartCrystal = 0;
    public const int DefaultStartOrePerClick = 1;
    public const int DefaultStartOrePerSecond = 0;
    public const int DefaultStartEnergyMax = 10;
    public const int DefaultStartEnergyRegenAmount = 1;
    public const float DefaultStartEnergyRegenInterval = 5f;
    public const float DefaultMinEnergyRegenInterval = 0.5f;
    public const int DefaultMetalPerCraft = 1;
    public const int DefaultMetalOreCost = 20;
    public const int DefaultMetalEnergyCost = 2;
    public const int DefaultOrePerCrystal = 1000;
    public const int DefaultMetalPerCrystal = 50;
    public const int DefaultPlatformCapacity = 100;
    public const int DefaultCapacity = 100;
    public const float DefaultTravelTimeSeconds = 60f;
    public const float DefaultBoostOfferAutoCloseSeconds = 5f;

    [Header("Resources")]
    [SerializeField] private int startOreAmount = DefaultStartOre;
    [SerializeField] private int startEnergy = DefaultStartEnergy;
    [SerializeField] private int startMetal = DefaultStartMetal;
    [SerializeField] private int startCrystal = DefaultStartCrystal;
    [SerializeField] private int startOrePerClick = DefaultStartOrePerClick;
    [SerializeField] private int startOrePerSecond = DefaultStartOrePerSecond;

    [Header("Energy")]
    [SerializeField] private int startEnergyMax = DefaultStartEnergyMax;
    [SerializeField] private int startEnergyRegenAmount = DefaultStartEnergyRegenAmount;
    [SerializeField] private float startEnergyRegenInterval = DefaultStartEnergyRegenInterval;
    [SerializeField] private float minEnergyRegenInterval = DefaultMinEnergyRegenInterval;

    [Header("Economy")]
    [SerializeField] private int metalPerCraft = DefaultMetalPerCraft;
    [SerializeField] private int metalOreCost = DefaultMetalOreCost;
    [SerializeField] private int metalEnergyCost = DefaultMetalEnergyCost;
    [SerializeField] private int orePerCrystal = DefaultOrePerCrystal;
    [SerializeField] private int metalPerCrystal = DefaultMetalPerCrystal;

    [Header("Shuttle")]
    [SerializeField] private int startPlatformCapacity = DefaultPlatformCapacity;
    [FormerlySerializedAs("startOre")]
    [SerializeField] private int shuttleStartOre = DefaultStartOre;
    [FormerlySerializedAs("capacity")]
    [SerializeField] private int shuttleCapacity = DefaultCapacity;
    [FormerlySerializedAs("travelTimeSeconds")]
    [SerializeField] private float shuttleTravelTimeSeconds = DefaultTravelTimeSeconds;

    [Header("Boost Offer")]
    [SerializeField] private float boostOfferAutoCloseSeconds = DefaultBoostOfferAutoCloseSeconds;

    public int StartOre => Mathf.Max(0, startOreAmount);
    public int StartEnergy => Mathf.Max(0, startEnergy);
    public int StartMetal => Mathf.Max(0, startMetal);
    public int StartCrystal => Mathf.Max(0, startCrystal);
    public int StartOrePerClick => Mathf.Max(0, startOrePerClick);
    public int StartOrePerSecond => Mathf.Max(0, startOrePerSecond);

    public int StartEnergyMax => Mathf.Max(1, startEnergyMax);
    public int StartEnergyRegenAmount => Mathf.Max(0, startEnergyRegenAmount);
    public float StartEnergyRegenInterval => Mathf.Max(0.1f, startEnergyRegenInterval);
    public float MinEnergyRegenInterval => Mathf.Max(0.1f, minEnergyRegenInterval);

    public int MetalPerCraft => Mathf.Max(1, metalPerCraft);
    public int MetalOreCost => Mathf.Max(0, metalOreCost);
    public int MetalEnergyCost => Mathf.Max(0, metalEnergyCost);
    public int OrePerCrystal => Mathf.Max(1, orePerCrystal);
    public int MetalPerCrystal => Mathf.Max(1, metalPerCrystal);
    public float OreToMetalRatio => MetalPerCraft > 0 ? MetalOreCost / (float)MetalPerCraft : 0f;

    public int StartPlatformCapacity => Mathf.Max(1, startPlatformCapacity);
    public int ShuttleStartOre => Mathf.Max(0, shuttleStartOre);
    public int Capacity => Mathf.Max(1, shuttleCapacity);
    public float TravelTimeSeconds => Mathf.Max(0f, shuttleTravelTimeSeconds);
    public float BoostOfferAutoCloseSeconds => Mathf.Max(0f, boostOfferAutoCloseSeconds);
}
