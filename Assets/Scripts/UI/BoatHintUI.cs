using System.Collections;
using TMPro;
using UnityEngine;

public class BoatModeHintUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text hintText;

    [SerializeField] private string text = "Press Space to launch";
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float visibleSeconds = 1f;
    [SerializeField] private float hiddenSeconds = 0.5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (hintText != null)
            hintText.text = text;

        canvasGroup.alpha = 0f;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(PulseRoutine());
    }

    public void Hide()
    {
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
        gameObject.SetActive(false);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}