using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatchResultRowUI : MonoBehaviour
{
    [SerializeField] private Image fishImage;
    [SerializeField] private TMP_Text fishNameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text moneyText;

    public void Initialize(FishData fishData, int quantity)
    {
        fishNameText.text = $"{fishData.fishName}";
        quantityText.text = $"x{quantity}";
        moneyText.text = $"${fishData.value * quantity:0}";

        if (fishImage != null)
        {
            fishImage.sprite = fishData.closeup;
            fishImage.enabled = fishData.closeup != null;
            fishImage.SetNativeSize();
        }
    }
}