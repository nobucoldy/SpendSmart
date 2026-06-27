using spendsmart.Constants;

namespace spendsmart.Services;

public class ReportService
{
    private readonly TransactionService transactionService;

    public ReportService(TransactionService transactionService)
    {
        this.transactionService = transactionService;
    }

    public MonthlyReport GetMonthlyReport(DateTime month)
    {
        var transactions = transactionService.GetTransactionsForMonth(month);
        var totalIncome = transactions
            .Where(transaction => transaction.Type == TransactionTypes.Income)
            .Sum(transaction => transaction.Amount);
        var totalExpense = transactions
            .Where(transaction => transaction.Type == TransactionTypes.Expense)
            .Sum(transaction => transaction.Amount);

        return new MonthlyReport
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            ExpenseCategories = BuildCategoryItems(
                transactions.Where(transaction => transaction.Type == TransactionTypes.Expense),
                totalExpense),
            IncomeCategories = BuildCategoryItems(
                transactions.Where(transaction => transaction.Type == TransactionTypes.Income),
                totalIncome)
        };
    }

    private static List<CategoryReportItem> BuildCategoryItems(
        IEnumerable<Models.Transaction> transactions,
        decimal typeTotal)
    {
        return transactions
            .GroupBy(transaction => new
            {
                CategoryName = transaction.Category?.Name ?? "Unknown",
                IconName = transaction.Category?.IconName ?? string.Empty,
                Color = transaction.Category?.Color ?? "#666666"
            })
            .Select(group =>
            {
                var total = group.Sum(transaction => transaction.Amount);
                return new CategoryReportItem
                {
                    CategoryName = group.Key.CategoryName,
                    IconName = group.Key.IconName,
                    Color = group.Key.Color,
                    TotalAmount = total,
                    Percentage = typeTotal <= 0 ? 0 : total / typeTotal * 100
                };
            })
            .OrderByDescending(item => item.TotalAmount)
            .ToList();
    }
}
