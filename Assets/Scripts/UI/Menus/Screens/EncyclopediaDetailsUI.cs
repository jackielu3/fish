using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaDetailsUI : MonoBehaviour
{
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private TMP_Text fishNameText;
    [SerializeField] private Image fishImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text baseValueText;
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text numberCaughtText;

    private void Awake()
    {
        if (detailsRoot != null)
            detailsRoot.SetActive(false);
    }

    public void ShowFish(FishData fishData)
    {
        if (fishData == null) return;

        if (detailsRoot != null)
            detailsRoot.SetActive(true);

        bool discovered = fishData.numberCaught > 0;

        fishImage.sprite = fishData.closeup;
        fishImage.SetNativeSize();

        if (discovered)
        {
            fishNameText.text = fishData.fishName;
            fishNameText.color = fishData.fishColor;

            fishImage.color = Color.white;

            descriptionText.text = fishData.description;

            baseValueText.text = $"Base Value: ${fishData.baseValue}";
            currentValueText.text = $"Current Value: ${fishData.currentValue}";
            numberCaughtText.text = $"Caught: {fishData.numberCaught}";
        }
        else
        {
            fishNameText.text = "???";
            fishNameText.color = Color.white;

            fishImage.color = Color.black;

            descriptionText.text = "???";

            baseValueText.text = "Base Value: ???";
            currentValueText.text = "Current Value: ???";
            numberCaughtText.text = "Caught: 0";
        }
    }
}
