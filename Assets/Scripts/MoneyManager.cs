using System.Collections;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField][ReadOnly] private float totalMoney;

    public float TotalMoney => totalMoney;
    public int BoatsOwned { get; private set; }

    private void Awake()
    {
        BoatsOwned = 0;
        totalMoney = 0f;
    }

    private void Start()
    {
        StartCoroutine(CalculatePassiveIncome());
    }

    private IEnumerator CalculatePassiveIncome()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            AddMoney(BoatsOwned * 5f);
        }
    }

    public void AddMoney(float amount)
    {
        if (amount <= 0f) return;

        totalMoney += amount;
    }

    public void OnMoneyEarned(Component sender, object data)
    {
        if (data is float amount)
        {
            AddMoney(amount);
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

        totalMoney -= amount;
        return true;
    }

    public void NewBoat()
    {
        BoatsOwned += 1;
    }
}