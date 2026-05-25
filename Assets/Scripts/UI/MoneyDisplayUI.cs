using TMPro;
using UnityEngine;

public class MoneyDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    public float totalMoney;

    private void Awake()
    {
        moneyText.text = "";
    }

    public void UpdateMoneyUI(Component sender, object data)
    {
        if (data is float valueInt)
        {
            totalMoney += valueInt;
            moneyText.text = "$" + totalMoney.ToString("F2");
        }
    }
}
