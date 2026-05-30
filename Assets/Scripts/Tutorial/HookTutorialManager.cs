
using System.Collections;
using TMPro;
using UnityEngine;

public class HookTutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private GameObject movementVisual;
    [SerializeField] private GameObject circleFishVisual;

    [Header("Timing")]
    [SerializeField] private float firstDelay = 1f;
    [SerializeField] private float secondDelay = 3f;
    [SerializeField] private float tutorialDisplaySeconds = 2f;
    [SerializeField] private float fadeDuration = 0.25f;

    private bool hasPlayedFirstHookTutorial;

    private void Awake()
    {
        HideAllInstant();
    }

    public void OnHookLaunched()
    {
        if (hasPlayedFirstHookTutorial)
            return;

        hasPlayedFirstHookTutorial = true;
        StartCoroutine(FirstHookTutorialRoutine());
    }

    private IEnumerator FirstHookTutorialRoutine()
    {
        yield return new WaitForSeconds(firstDelay);

        yield return ShowTutorial(
            "Use A and D to steer the hook",
            movementVisual,
            null
        );

        yield return new WaitForSeconds(secondDelay);

        yield return ShowTutorial(
            "Draw a circle around the fish you want to collect!",
            circleFishVisual,
            null
        );
    }

    private IEnumerator ShowTutorial(string text, GameObject visual, Transform worldTarget)
    {
        Time.timeScale = 0f;

        tutorialText.text = text;

        if (visual != null)
            visual.SetActive(true);

        if (worldTarget != null && visual != null)
            visual.transform.position = worldTarget.position;

        yield return FadeCanvas(1f);

        yield return new WaitForSecondsRealtime(tutorialDisplaySeconds);

        yield return FadeCanvas(0f);

        if (visual != null)
            visual.SetActive(false);

        Time.timeScale = 1f;
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = tutorialCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeDuration;

            tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        tutorialCanvasGroup.alpha = targetAlpha;
    }

    private void HideAllInstant()
    {
        tutorialCanvasGroup.alpha = 0f;

        if (movementVisual != null)
            movementVisual.SetActive(false);

        if (circleFishVisual != null)
            circleFishVisual.SetActive(false);
    }
}