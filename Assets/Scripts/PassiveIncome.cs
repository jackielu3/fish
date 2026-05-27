using System.Collections;
using UnityEngine;

public class PassiveIncome : MonoBehaviour
{
    private float totalMoney = 0;
    public int boatsOwned { get; private set; }

    [Header("Events")]
    [SerializeField] private GameEvent onMoneyEarned;

    private void Awake()
    {
        boatsOwned = 0;
    }

    void Start()
    {
        StartCoroutine(CalculatePassiveIncome());
    }

    IEnumerator CalculatePassiveIncome()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            onMoneyEarned.Raise(this, (float)boatsOwned * 5);
        }
    }

    public void NewBoat()
    {
        Debug.Log("NEW BOAT BOUGHT");

        boatsOwned += 1;
    }

    public float TotalMoney() => totalMoney;
}
