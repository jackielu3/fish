using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatchResultRowUI : MonoBehaviour
{
    [SerializeField] private Image fishImage;
    [SerializeField] private TMP_Text fishNameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text moneyText;

    public enum Type {
        fish,
        rocks,
        deductions
    }

    public Type fishType;

    public void Initialize(CatchReward reward)
    {
        fishNameText.text = reward.displayName;

        if (reward.amount <= 0)
        {
            fishType = Type.deductions;
        }
        else
        {
            fishType = Type.fish;
        }

        quantityText.text = $"x{reward.quantity}";

        moneyText.text = reward.amount >= 0f
            ? $"${reward.amount:0}"
            : $"-${Mathf.Abs(reward.amount):0}";

        if (fishImage != null)
        {
            fishImage.enabled = reward.data != null && reward.data.closeup != null;

            if (fishImage.enabled)
            {
                fishImage.sprite = reward.data.closeup;
                fishImage.SetNativeSize();
            }
        }
    }
}