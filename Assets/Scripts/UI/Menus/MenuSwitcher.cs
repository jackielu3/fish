using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MenuSwitcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private GameObject menuToOpen;
    [SerializeField] private TextMeshProUGUI plaqueText;

    [SerializeField] private string menuName;

    [Header("Movement")]
    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private float selectedYOffset = 25f;

    private RectTransform rectTransform;
    private Vector2 defaultAnchoredPosition;
    private Vector2 targetPosition;

    private bool isHovered;
    private bool isSelected;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        defaultAnchoredPosition = rectTransform.anchoredPosition;
        targetPosition = defaultAnchoredPosition;
    }

    private void Update()
    {
        if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) < 0.01f)
        {
            rectTransform.anchoredPosition = targetPosition;
            return;
        }

        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            lerpSpeed * Time.deltaTime
        );
    }

    private void OnEnable()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        UpdateTargetPosition();
        SnapToTarget();
    }

    public void OnClick()
    {
        menuManager.SwitchMenu(menuToOpen, this);
    }

    public void SetSelected(bool selected, bool snap = false)
    {
        isSelected = selected;

        if (plaqueText != null)
            plaqueText.text = menuName;

        UpdateTargetPosition();

        if (snap)
            SnapToTarget();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTargetPosition();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateTargetPosition();
    }

    private void UpdateTargetPosition()
    {
        if (isHovered || isSelected)
        {
            targetPosition = defaultAnchoredPosition + Vector2.up * selectedYOffset;
        }
        else
        {
            targetPosition = defaultAnchoredPosition;
        }
    }

    public void SnapToTarget()
    {
        rectTransform.anchoredPosition = targetPosition;
    }
}