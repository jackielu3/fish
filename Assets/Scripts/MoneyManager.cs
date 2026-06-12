using System.Collections;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private float totalMoney;

    public float TotalMoney => totalMoney;
    public int BoatsOwned { get; private set; }

    [SerializeField][ReadOnly] private float grossIncome;
    [SerializeField][ReadOnly] private float netProfits;

    public float GrossIncome => grossIncome;
    public float NetProfits => netProfits;
    public float MoneyLost => grossIncome - netProfits;

    private void Awake()
    {
        BoatsOwned = 0;
        totalMoney = 0f;
    }

    public void AddMoney(float amount, MoneyChangeType changeType = MoneyChangeType.Earned)
    {
        totalMoney += amount;

        switch (changeType)
        {
            case MoneyChangeType.Earned:
                if (amount > 0f)
                    grossIncome += amount;

                netProfits += amount;
                break;

            case MoneyChangeType.Deduction:
            case MoneyChangeType.Spending:
                netProfits += amount;
                break;

            case MoneyChangeType.LoanReceived:
                break;
        }
    }

    public void OnMoneyEarned(Component sender, object data)
    {
        if (data is MoneyChangeData moneyChange)
        {
            AddMoney(moneyChange.amount, moneyChange.changeType);
            return;
        }

        if (data is float amount)
        {
            AddMoney(amount, MoneyChangeType.Earned);
        }
    }

    public bool CanAfford(float amount)
    {
        return totalMoney >= amount;
    }

    public bool TrySpendMoney(float amount)
    {
        if (amount <= 0f) return false;
        if (!CanAfford(amount)) return false;

        AddMoney(-amount, MoneyChangeType.Spending);
        return true;
    }

    public bool TrySpendMoneyWithoutAffectingStats(float amount)
    {
        if (amount <= 0f) return false;
        if (!CanAfford(amount)) return false;

        totalMoney -= amount;
        return true;
    }

    public void NewBoat()
    {
        BoatsOwned += 1;
    }
}