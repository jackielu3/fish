using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialArrowSide
{
    Left,
    Right,
    Top,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [System.Serializable]
    public class TutorialStep
    {
        public string triggerId;

        public string topMessage;

        [TextArea]
        public string message;

        public Sprite image;

        [Header("Layout")]
        public RectTransform targetRect;
        public Vector2 panelAnchoredPosition;
        public Vector2 panelSize = new(500f, 220f);

        [Header("Arrow")]
        public bool useArrow;
        public RectTransform arrowTarget;
        public TutorialArrowSide arrowSide = TutorialArrowSide.Left;
        public float arrowDistanceFromTarget = 60f;
        public float arrowBobDistance = 12f;
        public float arrowBobSpeed = 3f;

        [Header("Timing")]
        public float lockedSeconds = 0.75f;

        [Header("Continue")]
        public bool useTargetButtonAsContinue;

        [Header("Next Tutorial")]
        public string nextTriggerId;
    }

    [Header("References")]
    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private RectTransform dimTop;
    [SerializeField] private RectTransform dimBottom;
    [SerializeField] private RectTransform dimLeft;
    [SerializeField] private RectTransform dimRight;
    [SerializeField] private RectTransform dimRoot;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private TMP_Text topMessageText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private RectTransform arrowRoot;
    [SerializeField] private Button continueButton;

    [Header("Highlight")]
    [SerializeField] private RectTransform highlightFrame;

    [Header("Steps")]
    [SerializeField] private List<TutorialStep> steps = new();

    [Header("Safety")]
    [SerializeField] private float maxTutorialSeconds = 20f;

    private readonly HashSet<string> completedTriggers = new();
    private readonly Queue<TutorialStep> queuedSteps = new();

    private bool continuePressed;
    private bool isPlaying;
    private TutorialStep currentStep;

    private Button currentTargetButton;
    private UnityEngine.Events.UnityAction targetButtonContinueAction;

    private void Awake()
    {
        Instance = this;
        HideInstant();

        continueButton.onClick.AddListener(() =>
        {
            continuePressed = true;
        });
    }

    private void Update()
    {
        if (!isPlaying || currentStep == null)
            return;

        UpdateArrowAnimation(currentStep);
    }

    public void TryPlay(string triggerId)
    {
        if (string.IsNullOrWhiteSpace(triggerId)) return;
        if (completedTriggers.Contains(triggerId)) return;

        TutorialStep step = steps.Find(s => s.triggerId == triggerId);

        if (step == null)
        {
            Debug.LogWarning($"No tutorial found for trigger: {triggerId}");
            return;
        }

        completedTriggers.Add(triggerId);
        queuedSteps.Enqueue(step);

        if (!isPlaying)
            StartCoroutine(PlayQueueRoutine());
    }

    private IEnumerator PlayQueueRoutine()
    {
        isPlaying = true;

        while (queuedSteps.Count > 0)
        {
            TutorialStep step = queuedSteps.Dequeue();
            yield return PlayStepRoutine(step);
        }

        isPlaying = false;
    }

    private IEnumerator PlayStepRoutine(TutorialStep step)
    {
        Time.timeScale = 0f;
        continuePressed = false;

        SetupStep(step);

        bool usingTargetButtonContinue = IsUsingTargetButtonContinue(step);

        CanvasGroup panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.blocksRaycasts = !usingTargetButtonContinue;
            panelCanvasGroup.interactable = !usingTargetButtonContinue;
        }

        rootCanvasGroup.alpha = 1f;
        rootCanvasGroup.blocksRaycasts = true;
        rootCanvasGroup.interactable = true;

        if (continueButton != null && !step.useTargetButtonAsContinue)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = false;
        }

        SetupTargetButtonContinue(step, false);

        if (step.lockedSeconds > 0f)
            yield return new WaitForSecondsRealtime(step.lockedSeconds);

        if (step.useTargetButtonAsContinue)
        {
            SetupTargetButtonContinue(step, true);
        }
        else if (continueButton != null)
        {
            if (continueButton != null)
                continueButton.interactable = !step.useTargetButtonAsContinue;
        }

        float maxTimer = 0f;

        while (!continuePressed)
        {
            maxTimer += Time.unscaledDeltaTime;

            if (maxTimer >= maxTutorialSeconds)
            {
                continuePressed = true;
                break;
            }

            if (step.useTargetButtonAsContinue && !IsTargetStillValid(step))
            {
                continuePressed = true;
                break;
            }

            yield return null;
        }

        CleanupTargetButtonContinue();

        string nextTriggerId = step.nextTriggerId;

        HideInstant();

        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(nextTriggerId))
            TryPlay(nextTriggerId);
    }

    private bool IsTargetStillValid(TutorialStep step)
    {
        if (step.targetRect == null)
            return false;

        if (!step.targetRect.gameObject.activeInHierarchy)
            return false;

        if (currentTargetButton == null)
            return false;

        if (!currentTargetButton.gameObject.activeInHierarchy)
            return false;

        return currentTargetButton.interactable;
    }

    private void SetupStep(TutorialStep step)
    {
        currentStep = step;

        if (topMessageText != null)
            topMessageText.text = step.topMessage;

        if (messageText != null)
            messageText.text = step.message;

        if (panelRoot != null)
        {
            panelRoot.anchoredPosition = step.panelAnchoredPosition;
            panelRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, step.panelSize.x);
            panelRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, step.panelSize.y);
        }

        if (tutorialImage != null)
        {
            tutorialImage.sprite = step.image;
            tutorialImage.gameObject.SetActive(step.image != null);
        }

        SetupHighlight(step.targetRect);
        SetupArrow(step);
    }

    private void SetupHighlight(RectTransform target)
    {
        if (target == null)
        {
            SetDimFullScreen();
            return;
        }

        Vector3[] worldCorners = new Vector3[4];
        target.GetWorldCorners(worldCorners);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dimRoot,
            RectTransformUtility.WorldToScreenPoint(null, worldCorners[0]),
            null,
            out Vector2 bottomLeft
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dimRoot,
            RectTransformUtility.WorldToScreenPoint(null, worldCorners[2]),
            null,
            out Vector2 topRight
        );

        float padding = 12f;

        bottomLeft -= Vector2.one * padding;
        topRight += Vector2.one * padding;

        Rect rootRect = dimRoot.rect;

        SetPanel(dimTop,
            new Vector2(0f, (topRight.y + rootRect.yMax) * 0.5f),
            new Vector2(rootRect.width, rootRect.yMax - topRight.y));

        SetPanel(dimBottom,
            new Vector2(0f, (rootRect.yMin + bottomLeft.y) * 0.5f),
            new Vector2(rootRect.width, bottomLeft.y - rootRect.yMin));

        SetPanel(dimLeft,
            new Vector2((rootRect.xMin + bottomLeft.x) * 0.5f, (bottomLeft.y + topRight.y) * 0.5f),
            new Vector2(bottomLeft.x - rootRect.xMin, topRight.y - bottomLeft.y));

        SetPanel(dimRight,
            new Vector2((topRight.x + rootRect.xMax) * 0.5f, (bottomLeft.y + topRight.y) * 0.5f),
            new Vector2(rootRect.xMax - topRight.x, topRight.y - bottomLeft.y));

        if (highlightFrame != null)
        {
            highlightFrame.gameObject.SetActive(true);
            highlightFrame.position = target.position;
            highlightFrame.sizeDelta = target.rect.size + Vector2.one * padding * 2f;
        }
    }

    private void SetPanel(RectTransform panel, Vector2 position, Vector2 size)
    {
        if (panel == null) return;

        panel.gameObject.SetActive(size.x > 0f && size.y > 0f);
        panel.anchoredPosition = position;
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    private void SetDimFullScreen()
    {
        if (dimRoot == null) return;

        Rect rootRect = dimRoot.rect;

        SetPanel(dimTop, Vector2.zero, Vector2.zero);
        SetPanel(dimBottom, Vector2.zero, Vector2.zero);
        SetPanel(dimLeft, Vector2.zero, Vector2.zero);

        SetPanel(dimRight, Vector2.zero, new Vector2(rootRect.width, rootRect.height));

        if (highlightFrame != null)
            highlightFrame.gameObject.SetActive(false);
    }

    private void SetupArrow(TutorialStep step)
    {
        if (arrowRoot == null) return;

        if (!step.useArrow || step.arrowTarget == null)
        {
            arrowRoot.gameObject.SetActive(false);
            return;
        }

        arrowRoot.gameObject.SetActive(true);
        UpdateArrowAnimation(step);
    }

    private void HideInstant()
    {
        CleanupTargetButtonContinue();

        CanvasGroup panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;
        }

        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = 0f;
            rootCanvasGroup.blocksRaycasts = false;
            rootCanvasGroup.interactable = false;
        }

        if (highlightFrame != null)
            highlightFrame.gameObject.SetActive(false);

        if (arrowRoot != null)
            arrowRoot.gameObject.SetActive(false);
    }

    private bool IsUsingTargetButtonContinue(TutorialStep step)
    {
        return step.useTargetButtonAsContinue && step.targetRect != null;
    }

    private void SetupTargetButtonContinue(TutorialStep step, bool enabled)
    {
        CleanupTargetButtonContinue();

        if (!enabled) return;
        if (!step.useTargetButtonAsContinue) return;
        if (step.targetRect == null) return;

        currentTargetButton = step.targetRect.GetComponent<Button>();

        if (currentTargetButton == null)
            currentTargetButton = step.targetRect.GetComponentInParent<Button>();

        if (currentTargetButton == null)
        {
            Debug.LogWarning($"Tutorial '{step.triggerId}' wants target button continue, but target has no Button.");
            return;
        }

        targetButtonContinueAction = () =>
        {
            continuePressed = true;
        };

        currentTargetButton.onClick.AddListener(targetButtonContinueAction);
    }

    private void CleanupTargetButtonContinue()
    {
        if (currentTargetButton != null && targetButtonContinueAction != null)
            currentTargetButton.onClick.RemoveListener(targetButtonContinueAction);

        currentTargetButton = null;
        targetButtonContinueAction = null;
    }

    private void UpdateArrowAnimation(TutorialStep step)
    {
        if (!step.arrowTarget.gameObject.activeInHierarchy)
        {
            arrowRoot.gameObject.SetActive(false);
            return;
        }

        if (arrowRoot == null || step.arrowTarget == null)
            return;

        Vector2 directionToTarget = GetDirectionToTarget(step.arrowSide);
        Vector2 awayFromTarget = -directionToTarget;

        float bob = Mathf.Sin(Time.unscaledTime * step.arrowBobSpeed) * step.arrowBobDistance;

        Vector2 offset =
            awayFromTarget * step.arrowDistanceFromTarget +
            awayFromTarget * bob;

        arrowRoot.position = (Vector2)step.arrowTarget.position + offset;

        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        arrowRoot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector2 GetDirectionToTarget(TutorialArrowSide side)
    {
        switch (side)
        {
            case TutorialArrowSide.Left:
                return Vector2.right;

            case TutorialArrowSide.Right:
                return Vector2.left;

            case TutorialArrowSide.Top:
                return Vector2.down;

            case TutorialArrowSide.Bottom:
                return Vector2.up;

            case TutorialArrowSide.TopLeft:
                return new Vector2(1f, -1f).normalized;

            case TutorialArrowSide.TopRight:
                return new Vector2(-1f, -1f).normalized;

            case TutorialArrowSide.BottomLeft:
                return new Vector2(1f, 1f).normalized;

            case TutorialArrowSide.BottomRight:
                return new Vector2(-1f, 1f).normalized;

            default:
                return Vector2.right;
        }
    }
}