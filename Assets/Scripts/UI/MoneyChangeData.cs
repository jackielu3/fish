public enum MoneyChangeType
{
    Earned,
    Deduction,
    Spending,
    LoanReceived
}

public class MoneyChangeData
{
    public float amount;
    public string label;
    public MoneyChangeType changeType;

    public MoneyChangeData(
        float amount,
        string label = "",
        MoneyChangeType changeType = MoneyChangeType.Earned
    )
    {
        this.amount = amount;
        this.label = label;
        this.changeType = changeType;
    }
}