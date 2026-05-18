using TMPro;
using UnityEngine;

public class MoneyDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private int totalMoney;

    private void Awake()
    {
        moneyText.text = "";
    }

    public void UpdateMoneyUI(Component sender, object data)
    {
        if (data is int valueInt)
        {
            totalMoney += valueInt;
            moneyText.text = "$" + totalMoney;
        }
    }
}
