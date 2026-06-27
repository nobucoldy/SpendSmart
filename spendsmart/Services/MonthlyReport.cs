namespace spendsmart.Services;

public sealed class MonthlyReport
{
    public decimal TotalIncome { get; init; }

    public decimal TotalExpense { get; init; }

    public decimal Balance => TotalIncome - TotalExpense;

    public List<CategoryReportItem> ExpenseCategories { get; init; } = new();

    public List<CategoryReportItem> IncomeCategories { get; init; } = new();
}

public sealed class CategoryReportItem
{
    public string CategoryName { get; init; } = string.Empty;

    public string IconName { get; init; } = string.Empty;

    public string Color { get; init; } = "#666666";

    public decimal TotalAmount { get; init; }

    public decimal Percentage { get; init; }
}
