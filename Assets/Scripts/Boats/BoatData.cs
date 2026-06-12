using System.Collections.Generic;
using UnityEngine;

public enum BoatEffectType
{
    FishValueBonus,
    FishValueMultiplier,
    UnlockSpecialFish,
    ReduceLineUsage,
    IncreaseHookTurnSpeed,
    FishFamilyBonus,
    RareFishChance
}

[System.Serializable]
public class BoatEffect
{
    public BoatEffectType effectType;

    [Header("Fish Target")]
    public FishData targetFish;

    [Header("Value")]
    public float amount;
    public float amountIncreasePerLevel;

    [Header("Special Fish Spawn Amount")]
    public int maxSpawnedIncreasePerLevel;
}

[CreateAssetMenu(fileName = "New Boat", menuName = "Boats/Boat Data")]
public class BoatData : ScriptableObject
{
    public string boatName;

    [TextArea]
    public string description;

    public Sprite boatSprite;

    public float cost;
    public float incomePerDive;

    [Header("Effect Display")]
    [TextArea]
    public string effectDescription;

    public List<BoatEffect> effects = new();

    [Header("Bank Upgrades")]
    public int maxLevel = 10;
    public float baseUpgradeCost = 100f;
    public float upgradeCostMultiplier = 1.5f;
    public float incomePerDiveIncreasePerLevel = 5f;
}
