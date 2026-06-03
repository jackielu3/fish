using TMPro;
using UnityEngine;

public class LineLengthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup hookCanvasGroup;
    private HookPathTracker hookPathTracker;
    [SerializeField] private TMP_Text lineText;

    private void Update()
    {
        if (hookPathTracker == null) return;

        lineText.text =
            $"{Mathf.CeilToInt(hookPathTracker.LineRemaining)} / {Mathf.CeilToInt(hookPathTracker.MaxLineLength)}";
    }

    public void SetHook(HookPathTracker tracker)
    {
        hookCanvasGroup.alpha = 1f;
        hookPathTracker = tracker;
    }

    public void ClearHook()
    {
        hookPathTracker = null;
        lineText.text = "";

        hookCanvasGroup.alpha = 0f;
    }
}