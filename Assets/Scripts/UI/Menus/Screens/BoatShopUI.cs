using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoatShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoatManager boatManager;
    [SerializeField] private MoneyManager moneyManager;

    [Header("Details UI")]
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private TMP_Text boatNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text passiveIncomeText;
    [SerializeField] private TMP_Text effectsText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;
    [SerializeField] private Image boatImage;

    [Header("Button")]
    [SerializeField] private Sprite buyBacking;
    [SerializeField] private Sprite notEnoughBacking;
    [SerializeField] private Color buyColor;
    [SerializeField] private Color notEnoughColor;

    private BoatData selectedBoat;

    private void Awake()
    {
        if (detailsRoot != null)
            detailsRoot.SetActive(false);

        buyButton.onClick.AddListener(BuySelectedBoat);
    }

    public void SelectBoat(BoatData boatData)
    {
        selectedBoat = boatData;

        if (detailsRoot != null)
            detailsRoot.SetActive(true);

        RefreshDetails();
    }

    private void OnEnable()
    {
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (selectedBoat == null) return;

        boatNameText.text = selectedBoat.boatName;
        descriptionText.text = selectedBoat.description;
        passiveIncomeText.text = $"Passive Income: ${selectedBoat.passiveIncomePerSecond:F2}/sec";
        costText.text = $"Cost: ${selectedBoat.cost:F2}";

        if (boatImage != null)
        {
            boatImage.sprite = selectedBoat.boatSprite;
        }

        effectsText.text = "";

        foreach (BoatEffect effect in selectedBoat.effects)
        {
            effectsText.text += $"• {effect.description}\n";
        }

        bool isOwned = boatManager.IsOwned(selectedBoat);

        if (isOwned)
        {
            buyButton.interactable = false;
            buyButtonText.text = "Owned";
            return;
        }

        bool canAfford = moneyManager.CanAfford(selectedBoat.cost);

        buyButton.interactable = canAfford;

        if (canAfford)
        {
            buyButton.image.sprite = buyBacking;
            buyButtonText.color = buyColor;
            buyButtonText.text = "Buy";
            buyButton.image.SetNativeSize();
        }
        else
        {
            buyButton.image.sprite = notEnoughBacking;
            buyButtonText.color = notEnoughColor;
            buyButtonText.text = "Not Enough Money";
            buyButton.image.SetNativeSize();
        }
    }

    private void BuySelectedBoat()
    {
        if (selectedBoat == null) return;

        bool bought = boatManager.TryBuyBoat(selectedBoat);

        if (bought)
            RefreshDetails();
    }
}