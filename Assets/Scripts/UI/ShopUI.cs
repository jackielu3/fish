using System.Collections;
using TMPro;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    // HORRIBLE, PLEASE REMOVE
    [SerializeField] private MoneyDisplayUI TEMP;
    [SerializeField] private PassiveIncome TEMPBOAT;

    [SerializeField] private IdleBoat[] idleBoats;
    private int boatIndex = 0;

    [SerializeField] private TextMeshProUGUI notEnoughMoneyText;
    [SerializeField] private TextMeshProUGUI boatsOwnedText;
    [SerializeField] private TextMeshProUGUI boatCostText;

    [SerializeField] private float boatPrice;
    [SerializeField] private float priceIncreasePerBoat;

    [Header("Events")]
    [SerializeField] private GameEvent onMoneyEarned;

    public void Awake()
    {
        boatCostText.text = $"${ boatPrice }";
    }

    public void BuyBoat()
    {
        Debug.Log("MONEY OWNED (ON THE SHOP UI SIDE)" +TEMP.totalMoney);

        if (TEMP.totalMoney < boatPrice) NotEnoughMoney();
        else {
            onMoneyEarned.Raise(this, -boatPrice);
            TEMPBOAT.NewBoat();
            boatsOwnedText.text = $"BOATS OWNED: {TEMPBOAT.boatsOwned}";

            boatPrice += priceIncreasePerBoat;
            boatCostText.text = $"${ boatPrice }";

            if (boatIndex < idleBoats.Length)
            {
                idleBoats[boatIndex].gameObject.SetActive(true);
                boatIndex++;
            }
        }
    }

    private void NotEnoughMoney()
    {
        notEnoughMoneyText.text = "HAHA YOU POOR, NOT ENOUGH MONEY XD";
        StartCoroutine(HideAfterDelay(1f));
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notEnoughMoneyText.text = "";
    }

    public void SUPERTEMP()
    {
        notEnoughMoneyText.text = "";
    }
}
