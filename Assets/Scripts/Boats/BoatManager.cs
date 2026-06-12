using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatManager : MonoBehaviour
{
    [System.Serializable]
    public class BoatEntry
    {
        public BoatData boatData;
        public GameObject backgroundBoatObject;
        public PlayerBoatVisual playerBoatObject;

        [ReadOnly] public bool isOwned;
        [ReadOnly] public int level = 1;
    }
    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private List<FishData> allFish = new();

    [Header("Boats")]
    [SerializeField] private List<BoatEntry> boats = new();
    [SerializeField][ReadOnly] private BoatData activeBoat;

    [Header("Events")]
    [SerializeField] private GameEvent onActiveBoatChanged;

    public BoatData ActiveBoat => activeBoat;
    public bool HasActiveBoat => activeBoat != null;

    private void Awake()
    {
        foreach (FishData fish in allFish)
        {
            fish.currentValue = fish.baseValue;
        }

        foreach (BoatEntry boat in boats)
        {
            if (boat.backgroundBoatObject != null)
                boat.backgroundBoatObject.SetActive(boat.isOwned);

            if (boat.playerBoatObject != null)
                boat.playerBoatObject.gameObject.SetActive(false);
        }
    }

    public bool IsActiveBoat(BoatData boatData)
    {
        return activeBoat == boatData;
    }

    public List<BoatEntry> GetAllBoatEntries()
    {
        return boats;
    }

    public bool TryBuyBoat(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);

        if (entry == null) return false;
        if (entry.isOwned) return false;
        if (!moneyManager.TrySpendMoneyWithoutAffectingStats(boatData.cost)) return false;
        entry.isOwned = true;

        if (entry.backgroundBoatObject != null)
            entry.backgroundBoatObject.SetActive(true);

        if (activeBoat == null)
        {
            activeBoat = boatData;
            onActiveBoatChanged.Raise(this, activeBoat);
        }

        RecalculateFishValues();

        return true;
    }

private void RecalculateFishValues()
{
    foreach (FishData fish in allFish)
    {
        float bonusPercent = 0f;
        float multiplier = 1f;

        if (activeBoat != null)
        {
            int activeLevel = GetBoatLevel(activeBoat);

            foreach (BoatEffect effect in activeBoat.effects)
            {
                if (effect.targetFish != fish)
                    continue;

                if (effect.effectType == BoatEffectType.FishValueBonus)
                {
                    bonusPercent += GetScaledEffectAmount(effect, activeLevel);
                }
                else if (effect.effectType == BoatEffectType.FishValueMultiplier)
                {
                    multiplier *= Mathf.Pow(effect.amount, activeLevel - 1);
                }
            }
        }

        fish.currentValue = fish.baseValue * (1f + bonusPercent) * multiplier;
    }
}

    public bool IsOwned(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);
        return entry != null && entry.isOwned;
    }

    public float GetIncomePerDive()
    {
        float total = 0f;

        foreach (BoatEntry entry in boats)
        {
            if (!entry.isOwned) continue;
            if (entry.boatData == null) continue;

            total += GetBoatIncomePerDive(entry.boatData);
        }

        return total;
    }

    private float GetScaledEffectAmount(BoatEffect effect, int boatLevel)
    {
        return effect.amount + effect.amountIncreasePerLevel * (boatLevel - 1);
    }

    public float GetActiveEffectAmount(BoatEffectType effectType)
    {
        if (activeBoat == null) return 0f;

        float total = 0f;
        int activeLevel = GetBoatLevel(activeBoat);

        foreach (BoatEffect effect in activeBoat.effects)
        {
            if (effect.effectType == effectType)
                total += GetScaledEffectAmount(effect, activeLevel);
        }

        return total;
    }

    public float GetActiveEffectAmount(BoatEffectType effectType, FishData targetFish)
    {
        if (activeBoat == null) return 0f;

        float total = 0f;
        int activeLevel = GetBoatLevel(activeBoat);

        foreach (BoatEffect effect in activeBoat.effects)
        {
            if (effect.effectType != effectType)
                continue;

            if (effect.targetFish != targetFish)
                continue;

            total += GetScaledEffectAmount(effect, activeLevel);
        }

        return total;
    }

    public int GetActiveSpecialFishSpawnBonus(FishData targetFish)
    {
        if (activeBoat == null) return 0;

        int total = 0;
        int activeLevel = GetBoatLevel(activeBoat);

        foreach (BoatEffect effect in activeBoat.effects)
        {
            if (effect.effectType != BoatEffectType.UnlockSpecialFish)
                continue;

            if (effect.targetFish != targetFish)
                continue;

            total += effect.maxSpawnedIncreasePerLevel * (activeLevel - 1);
        }

        return total;
    }

    public bool HasActiveEffect(BoatEffectType effectType)
    {
        return GetActiveEffectAmount(effectType) > 0f;
    }

    public bool HasActiveEffect(BoatEffectType effectType, FishData targetFish)
    {
        if (activeBoat == null) return false;

        foreach (BoatEffect effect in activeBoat.effects)
        {
            if (effect.effectType == effectType && effect.targetFish == targetFish)
                return true;
        }

        return false;
    }

    public BoatEntry GetBoatEntry(BoatData boatData)
    {
        return boats.Find(entry => entry.boatData == boatData);
    }

    public bool TrySwitchBoat(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);

        if (entry == null) return false;
        if (!entry.isOwned) return false;

        activeBoat = boatData;
        RecalculateFishValues();

        onActiveBoatChanged.Raise(this, activeBoat);

        return true;
    }

    public int GetBoatLevel(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);
        return entry == null ? 0 : entry.level;
    }

    public bool IsBoatMaxLevel(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);
        return entry == null || entry.level >= boatData.maxLevel;
    }

    public float GetBoatUpgradeCost(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);

        if (entry == null) return 0f;
        if (entry.level >= boatData.maxLevel) return 0f;

        return boatData.baseUpgradeCost *
               Mathf.Pow(boatData.upgradeCostMultiplier, entry.level - 1);
    }

    public bool TryUpgradeBoat(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);

        if (entry == null) return false;
        if (!entry.isOwned) return false;
        if (entry.level >= boatData.maxLevel) return false;

        float cost = GetBoatUpgradeCost(boatData);

        if (!moneyManager.TrySpendMoneyWithoutAffectingStats(cost))
            return false;

        entry.level++;

        RecalculateFishValues();

        if (activeBoat == boatData)
            onActiveBoatChanged.Raise(this, activeBoat);

        return true;
    }

    public float GetBoatIncomePerDive(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);

        if (entry == null || !entry.isOwned || boatData == null)
            return 0f;

        return boatData.incomePerDive +
               boatData.incomePerDiveIncreasePerLevel * (entry.level - 1);
    }

    public List<BoatEntry> GetOwnedBoatEntries()
    {
        List<BoatEntry> owned = new();

        foreach (BoatEntry entry in boats)
        {
            if (entry.isOwned)
                owned.Add(entry);
        }

        return owned;
    }

    public BoatEntry ActiveBoatEntry => GetBoatEntry(activeBoat);
}