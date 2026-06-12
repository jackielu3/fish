using System.Collections.Generic;
using UnityEngine;

public class SpecialFishUnlockManager : MonoBehaviour
{
    [System.Serializable]
    public class SpecialFishSpawnerEntry
    {
        public FishData fishData;
        public FishSpawner fishSpawner;
    }

    [System.Serializable]
    public class OysterSpawnerEntry
    {
        public FishData pearlFishData;
        public OysterSpawner oysterSpawner;
    }

    [Header("References")]
    [SerializeField] private BoatManager boatManager;

    [Header("Special Fish Spawners")]
    [SerializeField] private List<SpecialFishSpawnerEntry> specialFishSpawners = new();

    [SerializeField] private List<OysterSpawnerEntry> oysterSpawners = new();

    private void Start()
    {
        RefreshSpecialFish();
    }

    public void OnActiveBoatChanged(Component sender, object data)
    {
        RefreshSpecialFish();
    }

    private void RefreshSpecialFish()
    {
        foreach (SpecialFishSpawnerEntry entry in specialFishSpawners)
        {
            if (entry.fishData == null || entry.fishSpawner == null)
                continue;

            bool isUnlocked = IsSpecialFishUnlocked(entry.fishData);

            int spawnBonus = boatManager.GetActiveSpecialFishSpawnBonus(entry.fishData);
            entry.fishSpawner.SetMaxSpawnedBonus(spawnBonus);
            entry.fishSpawner.SetSpawnerActive(isUnlocked);
        }

        foreach (OysterSpawnerEntry entry in oysterSpawners)
        {
            if (entry.pearlFishData == null || entry.oysterSpawner == null)
                continue;

            bool isUnlocked = IsSpecialFishUnlocked(entry.pearlFishData);
            entry.oysterSpawner.SetSpawnerActive(isUnlocked);
        }
    }

    private bool IsSpecialFishUnlocked(FishData fishData)
    {
        BoatData activeBoat = boatManager.ActiveBoat;

        if (activeBoat == null)
            return false;

        foreach (BoatEffect effect in activeBoat.effects)
        {
            if (effect.effectType != BoatEffectType.UnlockSpecialFish)
                continue;

            if (effect.targetFish == fishData)
                return true;
        }

        return false;
    }
}