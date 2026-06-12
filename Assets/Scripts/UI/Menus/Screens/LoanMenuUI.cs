using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoanMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LoanManager loanManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private BankMenuUI bankMenuUI;

    [Header("Screens")]
    [SerializeField] private GameObject takeLoanRoot;
    [SerializeField] private GameObject activeLoanRoot;

    [Header("Shared Text")]
    [SerializeField] private TMP_Text currentBalanceText;
    [SerializeField] private TMP_Text availableCreditText;

    [Header("Take Loan Screen")]
    [SerializeField] private TMP_Text selectedLoanText;
    [SerializeField] private TMP_Text interestPreviewText;
    [SerializeField] private TMP_Text deductionPreviewText;
    [SerializeField] private TMP_Text totalRepaymentPreviewText;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button takeLoanButton;

    [Header("Active Loan Screen")]
    [SerializeField] private TMP_Text amountOwedText;
    [SerializeField] private TMP_Text activeInterestText;
    [SerializeField] private TMP_Text activeDeductionText;
    [SerializeField] private TMP_Text principalText;
    [SerializeField] private Button payOffButton;

    [Header("Values")]
    [SerializeField] private float loanStep = 100f;

    private float selectedLoanAmount;

    private void Awake()
    {
        increaseButton.onClick.AddListener(IncreaseLoan);
        decreaseButton.onClick.AddListener(DecreaseLoan);
        takeLoanButton.onClick.AddListener(TakeLoan);
        payOffButton.onClick.AddListener(PayOffLoan);
    }

    private void OnEnable()
    {
        selectedLoanAmount = loanManager.GetClampedLoanAmount(loanStep);
        Refresh();
    }

    private void IncreaseLoan()
    {
        selectedLoanAmount = loanManager.GetClampedLoanAmount(selectedLoanAmount + loanStep);
        Refresh();
    }

    private void DecreaseLoan()
    {
        selectedLoanAmount = loanManager.GetClampedLoanAmount(selectedLoanAmount - loanStep);
        Refresh();
    }

    private void TakeLoan()
    {
        if (!loanManager.TryTakeLoan(selectedLoanAmount))
            return;

        // TutorialManager.Instance.TryPlay("AfterLoan");

        bankMenuUI.Refresh();
        Refresh();
    }

    private void PayOffLoan()
    {
        if (!loanManager.TryPayOffLoan())
            return;

        bankMenuUI.Refresh();
        Refresh();
    }

    private void Refresh()
    {
        bool hasLoan = loanManager.HasActiveLoan;

        takeLoanRoot.SetActive(!hasLoan);
        activeLoanRoot.SetActive(hasLoan);

        RefreshSharedText();

        if (hasLoan)
            RefreshActiveLoanScreen();
        else
            RefreshTakeLoanScreen();
    }

    private void RefreshSharedText()
    {
        Debug.Log("Total Money: " + moneyManager.TotalMoney);
        Debug.Log("Max Loan Amount: " + loanManager.MaxLoanAmount);
        currentBalanceText.text = $"Current Balance:\n${moneyManager.TotalMoney:0}";
        availableCreditText.text = $"Available Credit:\n${loanManager.MaxLoanAmount:0}";
    }

    private void RefreshTakeLoanScreen()
    {
        selectedLoanAmount = loanManager.GetClampedLoanAmount(selectedLoanAmount);

        float amountOwed = loanManager.CalculateAmountOwed(selectedLoanAmount);
        float interestPercent = GetInterestPercent(selectedLoanAmount);
        float deductionPercent = loanManager.CalculateBankCutPercent(selectedLoanAmount) * 100f;

        selectedLoanText.text = $"${selectedLoanAmount:0}";
        interestPreviewText.text = $"Interest: x{interestPercent:0.#}%";
        deductionPreviewText.text = $"Deduction: {deductionPercent:0.#}%";
        totalRepaymentPreviewText.text = $"Total Repayment: ${amountOwed:0}";

        decreaseButton.interactable =
            loanManager.GetClampedLoanAmount(selectedLoanAmount - loanStep) < selectedLoanAmount;

        increaseButton.interactable =
            loanManager.GetClampedLoanAmount(selectedLoanAmount + loanStep) > selectedLoanAmount;

        takeLoanButton.interactable = selectedLoanAmount > 0f;
    }

    private void RefreshActiveLoanScreen()
    {
        float interestPercent = GetInterestPercentFromLoan();
        float deductionPercent = loanManager.BankCutPercent * 100f;

        amountOwedText.text = $"${loanManager.AmountOwed:0}";
        activeInterestText.text = $"Interest: x{interestPercent:0.#}%";
        activeDeductionText.text = $"Deduction: {deductionPercent:0.#}%";
        principalText.text = $"Principal: ${loanManager.BorrowedAmount:0}";

        payOffButton.interactable = moneyManager.CanAfford(loanManager.AmountOwed);
    }

    private float GetInterestPercent(float loanAmount)
    {
        float amountOwed = loanManager.CalculateAmountOwed(loanAmount);

        if (loanAmount <= 0f)
            return 0f;

        return (amountOwed / loanAmount) * 100f;
    }

    private float GetInterestPercentFromLoan()
    {
        if (loanManager.BorrowedAmount <= 0f)
            return 0f;

        return (loanManager.AmountOwed / loanManager.BorrowedAmount) * 100f;
    }
}