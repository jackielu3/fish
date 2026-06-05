using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitialLaunchChargeUI : MonoBehaviour
{
    [System.Serializable]
    public class ChargeSegment
    {
        public Image fillImage;
        public GameObject chargedOutline;
    }

    [SerializeField] private GameObject root;
    [SerializeField] private List<ChargeSegment> segments = new();

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        ResetSegments();
    }

    public void UpdateCharge(float chargeTime, float firstSegmentTime, float extraSegmentTime, int maxUnlockedSegmentIndex)
    {
        Show();

        for (int i = 0; i < segments.Count; i++)
        {
            bool isUnlocked = i <= maxUnlockedSegmentIndex;

            if (segments[i].fillImage != null)
                segments[i].fillImage.gameObject.SetActive(isUnlocked);

            if (segments[i].chargedOutline != null)
                segments[i].chargedOutline.SetActive(false);

            if (!isUnlocked)
                continue;

            float segmentStartTime = i == 0
                ? 0f
                : firstSegmentTime + extraSegmentTime * (i - 1);

            float segmentDuration = i == 0
                ? firstSegmentTime
                : extraSegmentTime;

            float rawProgress = Mathf.Clamp01((chargeTime - segmentStartTime) / segmentDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, rawProgress);

            if (segments[i].fillImage != null)
            {
                segments[i].fillImage.fillAmount = easedProgress;

                if (i % 2 == 0)
                    segments[i].fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                else
                    segments[i].fillImage.fillOrigin = (int)Image.OriginHorizontal.Right;
            }

            if (segments[i].chargedOutline != null)
                segments[i].chargedOutline.SetActive(rawProgress >= 1f);
        }
    }

    private void ResetSegments()
    {
        foreach (ChargeSegment segment in segments)
        {
            if (segment.fillImage != null)
                segment.fillImage.fillAmount = 0f;

            if (segment.chargedOutline != null)
                segment.chargedOutline.SetActive(false);
        }
    }
}