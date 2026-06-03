using TMPro;
using UnityEngine;

public class MoneyDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private MoneyManager moneyManager;

    private void Awake()
    {
        UpdateMoneyText();
    }

    private void Update()
    {
        UpdateMoneyText();
    }

    private void UpdateMoneyText()
    {
        if (moneyManager == null)
        {
            moneyText.text = "$0.00";
            return;
        }

        moneyText.text = "$" + moneyManager.TotalMoney.ToString("F2");
    }
}