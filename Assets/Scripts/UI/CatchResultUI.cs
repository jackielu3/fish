using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private Action onDismiss;
    private bool isShowing;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        continueButton.onClick.AddListener(Dismiss);
    }

    private void Update()
    {
        if (!isShowing) return;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            Dismiss();
        }
    }

    public void Show(CatchResult result, Action dismissedCallback)
    {
        if (result == null || result.fishCounts.Count == 0)
        {
            dismissedCallback?.Invoke();
            return;
        }

        onDismiss = dismissedCallback;
        isShowing = true;

        ClearRows();

        int rowIndex = 0;

        foreach (var pair in result.fishCounts)
        {
            FishData fishData = pair.Key;
            int quantity = pair.Value;

            CatchResultRowUI row = Instantiate(rowPrefab, rowsParent);
            row.Initialize(fishData, quantity);

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

    private void Dismiss()
    {
        if (!isShowing) return;

        isShowing = false;

        StopAllCoroutines();
        StartCoroutine(DismissRoutine());
    }

    private IEnumerator DismissRoutine()
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

    private IEnumerator FadeTo(float targetAlpha)
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