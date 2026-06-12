using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyDisplayUI : MonoBehaviour
{
    [System.Serializable]
    private class QueuedMoneyPopup
    {
        public float amount;
        public string label;

        public QueuedMoneyPopup(float amount, string label)
        {
            this.amount = amount;
            this.label = label;
        }
    }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private BoatModeUI boatModeUI;

    [Header("Popup")]
    [SerializeField] private TextMeshProUGUI popupTextPrefab;
    [SerializeField] private RectTransform popupParent;
    [SerializeField] private Vector2 popupStartOffset = new(0f, -40f);
    [SerializeField] private Vector2 popupEndOffset = new(0f, 0f);
    [SerializeField] private float popupDuration = 0.45f;
    [SerializeField] private float timeBetweenPopups = 0.12f;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    [Header("Money Count")]
    [SerializeField] private float countDuration = 0.45f;

    private float lastDisplayedMoney;
    private readonly Queue<QueuedMoneyPopup> popupQueue = new();

    private Coroutine popupRoutine;
    private Coroutine countRoutine;

    private float displayedMoney;

    private void Awake()
    {
        lastDisplayedMoney = moneyManager != null ? moneyManager.TotalMoney : 0f;
        SetMoneyText(lastDisplayedMoney);
    }

    private void Update()
    {
        if (moneyManager == null) return;

        if (!Mathf.Approximately(lastDisplayedMoney, moneyManager.TotalMoney))
        {
            lastDisplayedMoney = moneyManager.TotalMoney;
            SetMoneyText(lastDisplayedMoney);
        }
    }

    public void OnMoneyEarned(Component sender, object data)
    {
        if (data is MoneyChangeData moneyChange)
        {
            popupQueue.Enqueue(new QueuedMoneyPopup(moneyChange.amount, moneyChange.label));
            StartPopupQueue();

            AnimateMoneyToTarget();
            return;
        }

        if (data is float amount)
        {
            popupQueue.Enqueue(new QueuedMoneyPopup(amount, ""));
            StartPopupQueue();

            AnimateMoneyToTarget();
        }
    }

    private void StartPopupQueue()
    {
        if (popupRoutine == null)
            popupRoutine = StartCoroutine(PopupQueueRoutine());
    }

    private IEnumerator PopupQueueRoutine()
    {
        while (boatModeUI != null && !boatModeUI.IsVisible)
        {
            yield return null;
        }

        while (popupQueue.Count > 0)
        {
            QueuedMoneyPopup popup = popupQueue.Dequeue();
            yield return ShowPopupRoutine(popup);

            yield return new WaitForSeconds(timeBetweenPopups);
        }

        popupRoutine = null;
    }

    private IEnumerator ShowPopupRoutine(QueuedMoneyPopup popup)
    {
        TextMeshProUGUI popupText = Instantiate(popupTextPrefab, popupParent);

        RectTransform rect = popupText.GetComponent<RectTransform>();
        rect.anchoredPosition = popupStartOffset;

        bool isPositive = popup.amount >= 0f;
        popupText.color = isPositive ? positiveColor : negativeColor;

        string sign = isPositive ? "+" : "-";
        popupText.text = $"{sign}${Mathf.Abs(popup.amount):0}";

        if (!string.IsNullOrWhiteSpace(popup.label))
            popupText.text += $" {popup.label}";

        float timer = 0f;
        Color startColor = popupText.color;
        Color endColor = startColor;
        endColor.a = 0f;

        while (timer < popupDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / popupDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            rect.anchoredPosition = Vector2.Lerp(popupStartOffset, popupEndOffset, easedT);
            popupText.color = Color.Lerp(startColor, endColor, easedT);

            yield return null;
        }

        Destroy(popupText.gameObject);
    }

    private void AnimateMoneyToTarget()
    {
        if (moneyManager == null)
            return;

        if (countRoutine != null)
            StopCoroutine(countRoutine);

        countRoutine = StartCoroutine(CountMoneyRoutine(moneyManager.TotalMoney));
    }

    private IEnumerator CountMoneyRoutine(float targetMoney)
    {
        float startMoney = displayedMoney;
        float timer = 0f;

        while (timer < countDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / countDuration);

            displayedMoney = Mathf.Lerp(startMoney, targetMoney, t);
            SetMoneyText(displayedMoney);

            yield return null;
        }

        displayedMoney = targetMoney;
        SetMoneyText(displayedMoney);
        countRoutine = null;
    }

    private void SetMoneyText(float value)
    {
        moneyText.text = FormatMoney(value);
    }

    private string FormatMoney(float value)
    {
        float absValue = Mathf.Abs(value);
        string sign = value < 0f ? "-" : "";

        return $"{sign}${absValue:###,##0.00}";
    }
}