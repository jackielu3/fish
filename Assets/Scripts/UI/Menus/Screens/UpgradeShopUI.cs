using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private MoneyManager moneyManager;

    [Header("Details UI")]
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private TMP_Text upgradeNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text currentTierText;
    [SerializeField] private TMP_Text nextTierText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    [Header("Button")]
    [SerializeField] private Sprite buyBacking;
    [SerializeField] private Sprite notEnoughBacking;
    [SerializeField] private Sprite limitBacking;
    [SerializeField] private Color buyColor;
    [SerializeField] private Color notEnoughColor;
    [SerializeField] private Color limitColor;

    private UpgradeType selectedUpgradeType;

    private void Awake()
    {
        if (detailsRoot != null)
            detailsRoot.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshDetails();
    }

    public void SelectRopeLengthUpgrade()
    {
        SelectUpgrade(UpgradeType.RopeLength);
    }

    public void SelectHookSpeedUpgrade()
    {
        SelectUpgrade(UpgradeType.HookSpeed);
    }

    public void SelectInitialDiveLaunchUpgrade()
    {
        SelectUpgrade(UpgradeType.InitialDiveLaunch);
    }

    private void SelectUpgrade(UpgradeType type)
    {
        selectedUpgradeType = type;

        if (detailsRoot != null)
            detailsRoot.SetActive(true);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuySelectedUpgrade);

        RefreshDetails();
    }

    private void RefreshDetails()
    {
        UpgradeData upgrade = upgradeManager.GetUpgrade(selectedUpgradeType);

        if (upgrade == null)
            return;

        upgradeNameText.text = upgrade.upgradeDisplayName;

        UpgradeTier currentTier = upgrade.CurrentTier;
        UpgradeTier nextTier = upgrade.NextTier;

        currentTierText.text = $"Current: {currentTier.tierName} ({currentTier.value})";

        if (upgrade.IsMaxTier)
        {
            descriptionText.text = currentTier.description;
            nextTierText.text = "Next: Max Level";
            costText.text = "Cost: --";

            buyButton.interactable = false;
            buyButton.image.sprite = limitBacking;
            buyButtonText.color = limitColor;
            buyButtonText.text = "Max Level";
            buyButton.image.SetNativeSize();
            return;
        }

        descriptionText.text = nextTier.description;
        nextTierText.text = $"Next: {nextTier.tierName} ({nextTier.value})";
        costText.text = $"Cost: ${nextTier.cost:F2}";

        bool canAfford = moneyManager.CanAfford(nextTier.cost);

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

    private void BuySelectedUpgrade()
    {
        bool boughtUpgrade = upgradeManager.TryBuyUpgrade(selectedUpgradeType, moneyManager);

        if (boughtUpgrade)
        {
            RefreshDetails();
        }
    }
}