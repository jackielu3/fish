using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineLengthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup hookCanvasGroup;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private Image fillImage;

    [SerializeField] private float fillSmoothSpeed = 5f;
    [SerializeField] private float fadeSpeed = 5f;

    private HookPathTracker hookPathTracker;
    private float targetAlpha = 0f;
    private float currentFillAmount = 1f;

    private void Update()
    {
        hookCanvasGroup.alpha = Mathf.Lerp(hookCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        if (hookPathTracker == null) return;

        int remaining = Mathf.CeilToInt(hookPathTracker.LineRemaining);
        int max = Mathf.CeilToInt(hookPathTracker.MaxLineLength);
        lineText.text = $"{remaining - 1} / {max - 1}";

        float targetPercent = max > 0 ? (float)remaining / max : 0f;
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetPercent, Time.deltaTime * fillSmoothSpeed);
        fillImage.fillAmount = currentFillAmount;
    }

    public void SetHook(HookPathTracker tracker)
    {
        targetAlpha = 1f;
        hookPathTracker = tracker;
    }

    public void ClearHook()
    {
        hookPathTracker = null;
        lineText.text = "";

        targetAlpha = 0f;
    }
}