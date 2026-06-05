using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaitShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaitInventoryManager baitInventoryManager;
    [SerializeField] private MoneyManager moneyManager;

    [Header("Details UI")]
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    [SerializeField] private TMP_Text[] unused;

    [Header("Button Visuals")]
    [SerializeField] private Sprite buyBacking;
    [SerializeField] private Sprite notEnoughBacking;
    [SerializeField] private Sprite limitBacking;
    [SerializeField] private Color buyColor;
    [SerializeField] private Color notEnoughColor;
    [SerializeField] private Color limitColor;

    [Header("Bait Display")]
    [SerializeField] private string baitName = "Bait";
    [TextArea]
    [SerializeField]
    private string baitDescription =
        "Drop this underwater to attract extra fish for one dive.";

    private void Awake()
    {
        if (detailsRoot != null)
            detailsRoot.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshDetails();
    }

    public void SelectBait()
    {
        if (detailsRoot != null)
            detailsRoot.SetActive(true);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuyBait);

        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (baitInventoryManager == null) return;

        itemNameText.text = baitName;
        descriptionText.text = baitDescription;
        countText.text = $"Owned: {baitInventoryManager.BaitCount}/{baitInventoryManager.MaxBaitCount}";
        costText.text = $"Cost: ${baitInventoryManager.BaitCost:F2}";

        if (baitInventoryManager.IsFull)
        {
            buyButton.interactable = false;
            buyButton.image.sprite = limitBacking;
            buyButtonText.color = limitColor;
            buyButtonText.text = "Full";
            buyButton.image.SetNativeSize();
            return;
        }

        bool canAfford = moneyManager.CanAfford(baitInventoryManager.BaitCost);
        buyButton.interactable = canAfford;

        for (int i = 0; i <= unused.Length - 1; i++)
        {
            unused[i].text = "";
        }

        if (canAfford)
        {
            buyButton.image.sprite = buyBacking;
            buyButtonText.color = buyColor;
            buyButtonText.text = "Buy";
        }
        else
        {
            buyButton.image.sprite = notEnoughBacking;
            buyButtonText.color = notEnoughColor;
            buyButtonText.text = "Not Enough Money";
        }

        buyButton.image.SetNativeSize();
    }

    private void BuyBait()
    {
        if (baitInventoryManager.TryBuyBait())
        {
            RefreshDetails();
        }
    }
}