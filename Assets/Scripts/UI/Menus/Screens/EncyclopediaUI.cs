using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaUI : MonoBehaviour
{
    [System.Serializable]
    public class FishEntry
    {
        public FishData fishData;
        public GameObject encyclopediaObject;
        public Button button;

        public Image fishImage;
        public TMP_Text fishNameText;
    }

    [Header("Fish Entries")]
    [SerializeField] private List<FishEntry> fishEntries = new();

    [Header("References")]
    [SerializeField] private EncyclopediaDetailsUI detailsUI;

    [SerializeField] private RectTransform content;
    [SerializeField] private Vector2 contentYOffset = new(0, 20f);

    private void OnEnable()
    {
        Refresh();

        if (content == null || contentYOffset == null) return;
        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.anchoredPosition = contentYOffset;
    }

    private void Refresh()
    {
        foreach (FishEntry entry in fishEntries)
        {
            if (entry.fishData == null || entry.encyclopediaObject == null)
                continue;

            bool discovered = entry.fishData.numberCaught > 0;

            if (discovered)
            {
                entry.fishImage.color = Color.white;
                entry.fishNameText.text = entry.fishData.fishName;
            }
            else
            {
                entry.fishImage.color = Color.black;
                entry.fishNameText.text = "???";
            }

            entry.encyclopediaObject.SetActive(true);

            if (entry.button != null)
            {
                FishData fishData = entry.fishData;

                entry.button.onClick.RemoveAllListeners();
                entry.button.onClick.AddListener(() =>
                {
                    detailsUI.ShowFish(fishData);
                });
            }
        }
    }
}
