using System.Collections;
using TMPro;
using UnityEngine;

public class BoatModeUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup boatCanvasGroup;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TMP_Text hintText;

    [SerializeField] private string text = "Press Space to launch";
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float visibleSeconds = 1f;
    [SerializeField] private float hiddenSeconds = 0.5f;

    public bool IsVisible => boatCanvasGroup != null && boatCanvasGroup.alpha >= 1f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (hintText != null)
            hintText.text = text;

        tutorialCanvasGroup.alpha = 0f;
    }

    public void Show()
    {
        boatCanvasGroup.alpha = 1f;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(PulseRoutine());
    }

    public void Hide()
    {
        boatCanvasGroup.alpha = 0f;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            yield return FadeTo(1f);
            yield return new WaitForSeconds(visibleSeconds);
            yield return FadeTo(0f);
            yield return new WaitForSeconds(hiddenSeconds);
        }
    }

    private IEnumerator HideRoutine()
    {
        yield return FadeTo(0f);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = tutorialCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        tutorialCanvasGroup.alpha = targetAlpha;
    }
}