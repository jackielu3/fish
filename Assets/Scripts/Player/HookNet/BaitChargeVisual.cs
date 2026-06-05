using UnityEngine;
using UnityEngine.UI;

public class BaitChargeVisual : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image fillImage;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = Color.gray;
    [SerializeField] private Color fullColor = Color.green;

    private void Awake()
    {
        Hide();
    }

    public void SetCharge(float normalizedCharge)
    {
        normalizedCharge = Mathf.Clamp01(normalizedCharge);

        if (root != null)
            root.SetActive(normalizedCharge > 0f);

        fillImage.fillAmount = normalizedCharge;
        fillImage.color = Color.Lerp(emptyColor, fullColor, normalizedCharge);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = emptyColor;
        }
    }
}