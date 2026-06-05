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
        }
    }

    private void Start()
    {
        StartCoroutine(PassiveIncomeRoutine());
    }

    public bool IsActiveBoat(BoatData boatData)
    {
        return activeBoat == boatData;
    }

    private IEnumerator PassiveIncomeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            float passiveIncome = GetPassiveIncome();

            if (passiveIncome > 0f)
                moneyManager.AddMoney(passiveIncome);
        }
    }

    public bool TryBuyBoat(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);

        if (entry == null) return false;
        if (entry.isOwned) return false;
        if (!moneyManager.TrySpendMoney(boatData.cost)) return false;

        entry.isOwned = true;

        if (entry.backgroundBoatObject != null)
            entry.backgroundBoatObject.SetActive(true);

        if (activeBoat == null)
            activeBoat = boatData;

        RecalculateFishValues();

        return true;
    }

    private void RecalculateFishValues()
    {
        foreach (FishData fish in allFish)
        {
            float bonusPercent = 0f;

            if (activeBoat != null)
            {
                foreach (BoatEffect effect in activeBoat.effects)
                {
                    if (effect.effectType != BoatEffectType.FishValueBonus)
                        continue;

                    if (effect.targetFish != fish)
                        continue;

                    bonusPercent += effect.amount;
                }
            }

            fish.currentValue = fish.baseValue * (1f + bonusPercent);
        }
    }

    public bool IsOwned(BoatData boatData)
    {
        BoatEntry entry = GetBoatEntry(boatData);
        return entry != null && entry.isOwned;
    }

    public float GetPassiveIncome()
    {
        float total = 0f;

        foreach (BoatEntry entry in boats)
        {
            if (!entry.isOwned) continue;
            if (entry.boatData == null) continue;

            total += entry.boatData.passiveIncomePerSecond;
        }

        return total;
    }

    public float GetActiveEffectAmount(BoatEffectType effectType)
    {
        if (activeBoat == null) return 0f;

        float total = 0f;

        foreach (BoatEffect effect in activeBoat.effects)
        {
            if (effect.effectType == effectType)
                total += effect.amount;
        }

        return total;
    }

    public bool HasActiveEffect(BoatEffectType effectType)
    {
        return GetActiveEffectAmount(effectType) > 0f;
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
}