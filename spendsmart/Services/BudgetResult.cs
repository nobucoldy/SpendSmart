using spendsmart.Models;

namespace spendsmart.Services;

public sealed class BudgetResult
{
    private BudgetResult(bool success, string message, Budget? budget = null)
    {
        Success = success;
        Message = message;
        Budget = budget;
    }

    public bool Success { get; }

    public string Message { get; }

    public Budget? Budget { get; }

    public static BudgetResult Ok(Budget budget, string message)
    {
        return new BudgetResult(true, message, budget);
    }

    public static BudgetResult Ok(string message)
    {
        return new BudgetResult(true, message);
    }

    public static BudgetResult Fail(string message)
    {
        return new BudgetResult(false, message);
    }
}
