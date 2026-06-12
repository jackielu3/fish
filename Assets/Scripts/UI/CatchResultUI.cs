using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatchResultsUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform rowsParent;
    [SerializeField] private CatchResultRowUI rowPrefab;
    [SerializeField] private TMP_Text totalMoneyText;
    [SerializeField] private Button continueButton;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float rowSpacing = 80f;

    [Header("Events")]
    [SerializeField] private GameEvent onMoneyEarned;

    private Action onDismiss;
    private bool isShowing;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isShowing) return;
    }

    public void Show(CatchResult result, Action dismissedCallback)
    {
        if (result == null || !result.HasCatchRewards)
        {
            if (result != null)
                ApplyResultMoney(result);

            dismissedCallback?.Invoke();
            return;
        }

        onDismiss = dismissedCallback;
        isShowing = true;

        ClearRows();

        int rowIndex = 0;

        ApplyResultMoney(result);

        foreach (CatchReward reward in result.rewards)
        {
            CatchResultRowUI row = Instantiate(rowPrefab, rowsParent);
            row.Initialize(reward);

            RectTransform rowRect = row.GetComponent<RectTransform>();

            if (rowRect != null)
            {
                rowRect.anchoredPosition = new Vector2(
                    rowRect.anchoredPosition.x,
                    -rowSpacing * rowIndex
                );
            }

            rowIndex++;
        }

        if (totalMoneyText != null)
        {
            totalMoneyText.text = $"Total earned: ${result.totalMoney:0}";
        }

        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(FadeTo(1f));
    }

    public void Dismiss()
    {
        if (!isShowing) return;

        isShowing = false;

        StopAllCoroutines();
        StartCoroutine(DismissRoutine());
    }

    private void ApplyResultMoney(CatchResult result)
    {
        foreach (CatchReward reward in result.rewards)
        {
            onMoneyEarned.Raise(
                this,
                new MoneyChangeData(
                    reward.amount,
                    reward.displayName,
                    reward.rewardType == CatchRewardType.LoanPayment
                        ? MoneyChangeType.Deduction
                        : MoneyChangeType.Earned
                ));
        }
    }

    public IEnumerator DismissRoutine()
    {
        yield return FadeTo(0f);

        gameObject.SetActive(false);

        Action callback = onDismiss;
        onDismiss = null;

        callback?.Invoke();
    }

    private void ClearRows()
    {
        for (int i = rowsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(rowsParent.GetChild(i).gameObject);
        }
    }

    public IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}