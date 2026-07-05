using Microsoft.EntityFrameworkCore;
using spendsmart.Constants;
using spendsmart.Data;
using spendsmart.Models;

namespace spendsmart.Services;

public class BudgetService
{
    private const int PredictionMonthsToAverage = 3;

    private readonly ApplicationState applicationState;

    public BudgetService(ApplicationState applicationState)
    {
        this.applicationState = applicationState;
    }

    public BudgetResult SetBudget(int? categoryId, int year, int month, decimal limitAmount)
    {
        if (!applicationState.IsLoggedIn)
        {
            return BudgetResult.Fail("You must login first.");
        }

        if (month is < 1 or > 12)
        {
            return BudgetResult.Fail("Month is invalid.");
        }

        if (limitAmount <= 0)
        {
            return BudgetResult.Fail("Budget limit must be greater than zero.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;

        if (categoryId.HasValue)
        {
            var categoryExists = dbContext.Categories.Any(category =>
                category.CategoryId == categoryId.Value
                && category.UserId == userId
                && category.Type == TransactionTypes.Expense);

            if (!categoryExists)
            {
                return BudgetResult.Fail("Category was not found.");
            }
        }

        var budget = dbContext.Budgets.FirstOrDefault(budget =>
            budget.UserId == userId
            && budget.CategoryId == categoryId
            && budget.Year == year
            && budget.Month == month);

        if (budget is null)
        {
            budget = new Budget
            {
                UserId = userId,
                CategoryId = categoryId,
                Year = year,
                Month = month,
                LimitAmount = limitAmount,
                CreatedAt = DateTime.Now
            };

            dbContext.Budgets.Add(budget);
        }
        else
        {
            budget.LimitAmount = limitAmount;
        }

        dbContext.SaveChanges();

        return BudgetResult.Ok(budget, "Budget saved successfully.");
    }

    public BudgetResult DeleteBudget(int budgetId)
    {
        if (!applicationState.IsLoggedIn)
        {
            return BudgetResult.Fail("You must login first.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;

        var budget = dbContext.Budgets.FirstOrDefault(budget =>
            budget.BudgetId == budgetId && budget.UserId == userId);

        if (budget is null)
        {
            return BudgetResult.Fail("Budget was not found.");
        }

        dbContext.Budgets.Remove(budget);
        dbContext.SaveChanges();

        return BudgetResult.Ok("Budget deleted successfully.");
    }

    public List<BudgetStatus> GetBudgetStatuses(int year, int month)
    {
        if (!applicationState.IsLoggedIn)
        {
            return new List<BudgetStatus>();
        }

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;

        var budgets = dbContext.Budgets
            .AsNoTracking()
            .Include(budget => budget.Category)
            .Where(budget => budget.UserId == userId && budget.Year == year && budget.Month == month)
            .ToList();

        if (budgets.Count == 0)
        {
            return new List<BudgetStatus>();
        }

        var expenseTransactions = dbContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId
                && transaction.Type == TransactionTypes.Expense
                && transaction.Date >= startDate
                && transaction.Date < endDate)
            .ToList();

        var totalExpense = expenseTransactions.Sum(transaction => transaction.Amount);

        return budgets
            .OrderBy(budget => budget.CategoryId.HasValue ? 1 : 0)
            .ThenBy(budget => budget.Category?.Name)
            .Select(budget =>
            {
                var spent = budget.CategoryId.HasValue
                    ? expenseTransactions
                        .Where(transaction => transaction.CategoryId == budget.CategoryId.Value)
                        .Sum(transaction => transaction.Amount)
                    : totalExpense;

                return new BudgetStatus
                {
                    BudgetId = budget.BudgetId,
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.Category?.Name ?? "Tổng chi tiêu",
                    LimitAmount = budget.LimitAmount,
                    SpentAmount = spent,
                    PercentageUsed = budget.LimitAmount <= 0 ? 0 : spent / budget.LimitAmount * 100
                };
            })
            .ToList();
    }

    /// <summary>
    /// Predicts next month's expense using a simple moving average of the last few months of actual spending.
    /// Pass a categoryId to predict for a single category, or null to predict overall expense.
    /// </summary>
    public decimal PredictNextMonthExpense(int? categoryId = null)
    {
        if (!applicationState.IsLoggedIn)
        {
            return 0m;
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var referenceMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var monthlyTotals = new List<decimal>();

        for (var offset = 1; offset <= PredictionMonthsToAverage; offset++)
        {
            var monthStart = referenceMonth.AddMonths(-offset);
            var monthEnd = monthStart.AddMonths(1);

            var query = dbContext.Transactions
                .AsNoTracking()
                .Where(transaction =>
                    transaction.UserId == userId
                    && transaction.Type == TransactionTypes.Expense
                    && transaction.Date >= monthStart
                    && transaction.Date < monthEnd);

            if (categoryId.HasValue)
            {
                query = query.Where(transaction => transaction.CategoryId == categoryId.Value);
            }

            var total = query.Sum(transaction => (decimal?)transaction.Amount) ?? 0m;
            monthlyTotals.Add(total);
        }

        if (monthlyTotals.Count == 0)
        {
            return 0m;
        }

        return Math.Round(monthlyTotals.Average(), 0);
    }
}
