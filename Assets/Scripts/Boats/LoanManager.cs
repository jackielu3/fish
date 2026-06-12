using UnityEngine;

public class LoanManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;

    [Header("Loan Settings")]
    [SerializeField] private float loanStep = 100f;
    [SerializeField] private float minLoanAmount = 100f;
    [SerializeField] private float extraLoanLimit = 100f;
    [SerializeField] private float maxHardLoanLimit = 100000f;

    [Header("Rates")]
    [SerializeField] private float baseRepayMultiplier = 1.1f;
    [SerializeField] private float maxRepayMultiplier = 1.5f;
    [SerializeField] private float minBankCutPercent = 0.03f;
    [SerializeField] private float maxBankCutPercent = 0.25f;

    [SerializeField][ReadOnly] private bool hasActiveLoan;
    [SerializeField][ReadOnly] private float borrowedAmount;
    [SerializeField][ReadOnly] private float amountOwed;
    [SerializeField][ReadOnly] private float bankCutPercent;

    public bool HasActiveLoan => hasActiveLoan;
    public float BorrowedAmount => borrowedAmount;
    public float AmountOwed => amountOwed;
    public float BankCutPercent => bankCutPercent;

    public float MaxLoanAmount
    {
        get
        {
            float limit = moneyManager.NetProfits + extraLoanLimit;
            return Mathf.Clamp(limit, minLoanAmount, maxHardLoanLimit);
        }
    }

    public float GetClampedLoanAmount(float requestedAmount)
    {
        float stepped = Mathf.Round(requestedAmount / loanStep) * loanStep;
        return Mathf.Clamp(stepped, minLoanAmount, MaxLoanAmount);
    }

    public float CalculateRepayMultiplier(float loanAmount)
    {
        float t = Mathf.InverseLerp(minLoanAmount, MaxLoanAmount, loanAmount);
        return Mathf.Lerp(baseRepayMultiplier, maxRepayMultiplier, t);
    }

    public float CalculateBankCutPercent(float loanAmount)
    {
        float t = Mathf.InverseLerp(minLoanAmount, MaxLoanAmount, loanAmount);
        return Mathf.Lerp(minBankCutPercent, maxBankCutPercent, t);
    }

    public float CalculateAmountOwed(float loanAmount)
    {
        return loanAmount * CalculateRepayMultiplier(loanAmount);
    }

    public bool TryTakeLoan(float requestedAmount)
    {
        if (hasActiveLoan) return false;

        float loanAmount = GetClampedLoanAmount(requestedAmount);

        borrowedAmount = loanAmount;
        amountOwed = CalculateAmountOwed(loanAmount);
        bankCutPercent = CalculateBankCutPercent(loanAmount);
        hasActiveLoan = true;

        moneyManager.AddMoney(loanAmount, MoneyChangeType.LoanReceived);
        return true;
    }

    public bool TryPayOffLoan()
    {
        if (!hasActiveLoan) return false;
        if (!moneyManager.TrySpendMoney(amountOwed)) return false;

        borrowedAmount = 0f;
        amountOwed = 0f;
        bankCutPercent = 0f;
        hasActiveLoan = false;

        return true;
    }

    public float CalculateDiveCut(float positiveDiveTotal)
    {
        if (!hasActiveLoan) return 0f;
        if (positiveDiveTotal <= 0f) return 0f;

        return positiveDiveTotal * bankCutPercent;
    }
}