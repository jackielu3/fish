using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BankMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoatManager boatManager;
    [SerializeField] private BoatMovement boatMovement;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private LoanManager loanManager;
    [SerializeField] private CanvasGroup bankInteractableGroup;

    [SerializeField] private GameObject loanRoot;

    [Header("Boat List")]
    [SerializeField] private List<Transform> boatButtonSlots = new();
    [SerializeField] private BankBoatButtonUI boatButtonPrefab;

    [Header("Bank Stats")]
    [SerializeField] private TMP_Text grossProfitsText;
    [SerializeField] private TMP_Text passiveIncomeText;
    [SerializeField] private TMP_Text loanDeductionsText;
    [SerializeField] private TMP_Text litteringFineText;
    [SerializeField] private TMP_Text netProfitsText;

    [Header("Selected Boat")]
    [SerializeField] private TMP_Text selectedBoatText;
    [SerializeField] private TMP_Text boatEffectsText;

    [Header("Upgrade")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeCostText;

    [Header("Values")]
    [SerializeField] private float litteringFine = 0f;

    private readonly List<BankBoatButtonUI> spawnedButtons = new();
    private BoatData selectedBoat;

    public BoatManager BoatManager => boatManager;

    private void Awake()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedBoat);
    }

    private void Update()
    {
        grossIncome();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        loanRoot.SetActive(false);
        SetLoanScreenOpen(false);
    }

    public void Refresh()
    {
        RefreshBoatList();
        grossIncome();

        if (boatManager.ActiveBoat != null)
            SelectBoat(boatManager.ActiveBoat);
        else
            ClearSelectedBoatPanel();
    }

    private void RefreshBoatList()
    {
        ClearSpawnedButtons();

        List<BoatManager.BoatEntry> ownedBoats = boatManager.GetOwnedBoatEntries();

        int count = Mathf.Min(ownedBoats.Count, boatButtonSlots.Count);

        for (int i = 0; i < count; i++)
        {
            if (boatButtonSlots[i] == null)
                continue;

            BankBoatButtonUI button = Instantiate(boatButtonPrefab, boatButtonSlots[i]);
            button.Initialize(ownedBoats[i].boatData, this);
            spawnedButtons.Add(button);
        }
    }

    private void ClearSpawnedButtons()
    {
        foreach (BankBoatButtonUI button in spawnedButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        spawnedButtons.Clear();
    }

    public void SelectBoat(BoatData boatData)
    {
        if (boatData == null) return;

        selectedBoat = boatData;

        boatManager.TrySwitchBoat(selectedBoat);
        boatMovement.RefreshActiveBoatVisual();

        RefreshSelectedBoatPanel();
    }

    private void RefreshSelectedBoatPanel()
    {
        if (selectedBoat == null)
        {
            ClearSelectedBoatPanel();
            return;
        }

        RefreshSelectedBoatEffects();
        RefreshUpgradeButton();
    }

    private void grossIncome()
    {
        float grossIncome = moneyManager.GrossIncome;
        float netProfits = moneyManager.NetProfits;
        float moneyLost = moneyManager.MoneyLost;
        float passiveIncome = Mathf.Max(0f, boatManager.GetIncomePerDive());

        float loanDeductionPercent = loanManager != null && loanManager.HasActiveLoan
            ? loanManager.BankCutPercent * 100f
            : 0f;

        grossProfitsText.text = $"Gross Income: ${grossIncome:0}";
        passiveIncomeText.text = $"Income Per Dive: ${passiveIncome:0}";
        loanDeductionsText.text = $"Loan Deductions: -{loanDeductionPercent:0.#}%";
        litteringFineText.text = $"Money Lost: ${moneyLost:0}";
        netProfitsText.text = $"Net Profits: ${netProfits:0}";
    }

    private void RefreshSelectedBoatEffects()
    {
        selectedBoatText.text =
            $"Selected Boat: {FormatBoat(selectedBoat.boatName)}";

        string effectText = "";

        bool hasShrimp = false;
        int totalShrimpSpawnBonus = 0;
        float shrimpBaseValue = 0f;
        float shrimpCurrentValue = 0f;

        foreach (BoatEffect effect in selectedBoat.effects)
        {
            if (effect.targetFish == null)
                continue;

            bool isShrimp =
                effect.targetFish.fishName == "Shrimp" ||
                effect.targetFish.fishName == "Shrimp 1";

            if (!isShrimp)
                continue;

            hasShrimp = true;

            if (effect.effectType == BoatEffectType.UnlockSpecialFish)
                totalShrimpSpawnBonus += boatManager.GetActiveSpecialFishSpawnBonus(effect.targetFish);

            if (effect.effectType == BoatEffectType.FishValueMultiplier)
            {
                shrimpBaseValue = effect.targetFish.baseValue;
                shrimpCurrentValue = effect.targetFish.currentValue;
            }
        }

        foreach (BoatEffect effect in selectedBoat.effects)
        {
            if (effect.targetFish == null)
                continue;

            bool isShrimp = effect.targetFish.fishName == "Shrimp" || effect.targetFish.fishName == "Shrimp 1";

            if (isShrimp)
                continue;

            float amount = boatManager.GetActiveEffectAmount(effect.effectType, effect.targetFish);

            switch (effect.effectType)
            {
                case BoatEffectType.FishValueBonus:
                    boatEffectsText.lineSpacing = -30;
                    effectText += $"{effect.targetFish.fishName} Value: +{amount * 100f:0.#}%\n";
                    effectText += $"${effect.targetFish.baseValue:0.00} → ${effect.targetFish.currentValue:0.00}\n";
                    break;

                case BoatEffectType.UnlockSpecialFish:
                    boatEffectsText.lineSpacing = -2;

                    int spawnBonus = boatManager.GetActiveSpecialFishSpawnBonus(effect.targetFish);

                    effectText += $"{effect.targetFish.fishName} Max Spawn Bonus: +{spawnBonus}\n";
                    break;

                case BoatEffectType.ReduceLineUsage:
                    boatEffectsText.lineSpacing = -2;
                    effectText += $"Line Usage Reduction: {amount * 100f:0.#}%\n";
                    break;

                case BoatEffectType.IncreaseHookTurnSpeed:
                    boatEffectsText.lineSpacing = -2;
                    effectText += $"Hook Speed: +{amount:0.#}%\n";
                    effectText += $"Hook Control: +{amount:0.#}%\n";
                    break;

                case BoatEffectType.RareFishChance:
                    boatEffectsText.lineSpacing = -2;
                    effectText += $"Rare Fish Chance: +{amount:0.#}%\n";
                    break;

                case BoatEffectType.FishValueMultiplier:
                    boatEffectsText.lineSpacing = -2;

                    effectText += $"{effect.targetFish.fishName} Value Multiplier\n";
                    effectText += $"${effect.targetFish.baseValue:0.00} → ${effect.targetFish.currentValue:0.00}\n";
                    break;
            }
        }

        if (hasShrimp)
        {
            effectText += $"Shrimp Max Spawn Bonus: +{totalShrimpSpawnBonus}\n";
            effectText += $"Shrimp Value\n";
            effectText += $"${shrimpBaseValue:0.00} → ${shrimpCurrentValue:0.00}\n";
        }

        boatEffectsText.text = effectText;
    }

    string FormatBoat(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToUpper(name[0]) + name.Substring(1).ToLower();
    }

    private void RefreshUpgradeButton()
    {
        if (boatManager.IsBoatMaxLevel(selectedBoat))
        {
            upgradeButton.interactable = false;
            upgradeCostText.text = "Upgrade Cost: MAX";
            return;
        }

        float cost = boatManager.GetBoatUpgradeCost(selectedBoat);

        upgradeButton.interactable = moneyManager.CanAfford(cost);
        upgradeCostText.text = $"Upgrade Cost: ${cost:0}";
    }

    private void UpgradeSelectedBoat()
    {
        if (selectedBoat == null) return;

        if (boatManager.TryUpgradeBoat(selectedBoat))
        {
            RefreshBoatList();
            RefreshSelectedBoatPanel();
        }
    }

    private void ClearSelectedBoatPanel()
    {
        grossProfitsText.text = "Gross Income: $0";
        passiveIncomeText.text = "Income Per Dive: $0";
        loanDeductionsText.text = "Loan Deductions: 0%";
        litteringFineText.text = $"Money Lost: ${litteringFine:0}";
        netProfitsText.text = "Net Profits: $0";

        selectedBoatText.text = "Selected Boat: None";
        boatEffectsText.text = "";

        upgradeButton.interactable = false;
        upgradeCostText.text = "Upgrade Cost: --";
    }

    public void OpenLoanScreen()
    {
        SetLoanScreenOpen(true);
        // TutorialManager.Instance.TryPlay("OpenedLoanScreen");
    }

    public void SetLoanScreenOpen(bool isOpen)
    {
        if (bankInteractableGroup != null)
        {
            bankInteractableGroup.interactable = !isOpen;
            bankInteractableGroup.blocksRaycasts = !isOpen;
        }

        if (loanRoot != null)
            loanRoot.SetActive(isOpen);
    }
}