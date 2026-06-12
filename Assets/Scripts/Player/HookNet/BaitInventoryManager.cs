using UnityEngine;

public class BaitInventoryManager : MonoBehaviour
{
    [SerializeField] private MoneyManager moneyManager;

    [Header("Bait")]
    [SerializeField] private int baitCount;
    [SerializeField] private int maxBaitCount = 10;
    [SerializeField] private float baitCost = 10f;

    public int BaitCount => baitCount;
    public int MaxBaitCount => maxBaitCount;
    public float BaitCost => baitCost;

    public bool HasBait => baitCount > 0;
    public bool IsFull => baitCount >= maxBaitCount;

    public bool TryBuyBait()
    {
        if (IsFull) return false;
        if (!moneyManager.TrySpendMoneyWithoutAffectingStats(baitCost)) return false;

        baitCount++;
        return true;
    }

    public bool TryUseBait()
    {
        if (baitCount <= 0) return false;

        baitCount--;
        return true;
    }
}