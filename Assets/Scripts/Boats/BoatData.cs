using System.Collections.Generic;
using UnityEngine;

public enum BoatEffectType
{
    FishValueBonus,
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

    [TextArea]
    public string description;
}

[CreateAssetMenu(fileName = "New Boat", menuName = "Boats/Boat Data")]
public class BoatData : ScriptableObject
{
    public string boatName;

    [TextArea]
    public string description;

    public Sprite boatSprite;

    public float cost;
    public float passiveIncomePerSecond;

    [Header("Player Boat")]
    public Vector2 hookSpawnLocalPosition;

    public List<BoatEffect> effects = new();
}
