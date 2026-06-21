namespace spendsmart.Constants;

public static class TransactionTypes
{
    public const string Income = "Income";
    public const string Expense = "Expense";

    public static bool IsValid(string? type)
    {
        return type is Income or Expense;
    }
}
