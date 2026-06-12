using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BankBoatButtonUI : MonoBehaviour
{
    [SerializeField] private Image boatImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text incomeText;
    [SerializeField] private Button button;

    private BoatData boatData;
    private BankMenuUI bankMenuUI;

    public void Initialize(BoatData data, BankMenuUI menu)
    {
        boatData = data;
        bankMenuUI = menu;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => bankMenuUI.SelectBoat(boatData));

        Refresh();
    }

    public void Refresh()
    {
        if (boatData == null || bankMenuUI == null) return;

        BoatManager boatManager = bankMenuUI.BoatManager;

        if (boatImage != null)
            boatImage.sprite = boatData.boatSprite;

        nameText.text = $"{boatData.boatName}";
        levelText.text = $"Lv. {boatManager.GetBoatLevel(boatData)}";
        float income = Mathf.Max(0f, boatManager.GetBoatIncomePerDive(boatData));
        incomeText.text = $"+${income:0}\nEvery Dive";
    }
}