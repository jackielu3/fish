using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoatShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoatManager boatManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private BoatMovement boatMovement;

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
    [SerializeField] private Sprite switchBacking;
    [SerializeField] private Sprite activeBacking;

    [SerializeField] private Color buyColor;
    [SerializeField] private Color notEnoughColor;
    [SerializeField] private Color switchColor;
    [SerializeField] private Color activeColor;

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
        passiveIncomeText.text = $"Income Per Dive: ${selectedBoat.incomePerDive:F2}";
        costText.text = $"Cost: ${selectedBoat.cost:F2}";

        if (boatImage != null)
        {
            boatImage.sprite = selectedBoat.boatSprite;
        }

        effectsText.text = selectedBoat.effectDescription;

        bool isOwned = boatManager.IsOwned(selectedBoat);

        if (isOwned)
        {
            bool isActive = boatManager.IsActiveBoat(selectedBoat);

            if (isActive)
            {
                buyButton.interactable = false;

                buyButton.image.sprite = activeBacking;
                buyButtonText.color = activeColor;
                buyButtonText.text = "Active";

                buyButton.image.SetNativeSize();
            }
            else
            {
                buyButton.interactable = true;

                buyButton.image.sprite = switchBacking;
                buyButtonText.color = switchColor;
                buyButtonText.text = "Switch";

                buyButton.image.SetNativeSize();
            }

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
        if (selectedBoat == null)
            return;

        bool isOwned = boatManager.IsOwned(selectedBoat);

        if (isOwned)
        {
            boatManager.TrySwitchBoat(selectedBoat);
            boatMovement.RefreshActiveBoatVisual();
            RefreshDetails();

            return;
        }

        bool bought = boatManager.TryBuyBoat(selectedBoat);

        if (bought)
        {
            // TutorialManager.Instance.TryPlay("BoughtFirstBoat");
            boatManager.TrySwitchBoat(selectedBoat);
            boatMovement.RefreshActiveBoatVisual();
            RefreshDetails();
        }
    }
}