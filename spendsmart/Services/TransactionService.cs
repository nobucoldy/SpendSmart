using Microsoft.EntityFrameworkCore;
using spendsmart.Constants;
using spendsmart.Data;
using spendsmart.Models;

namespace spendsmart.Services;

public class TransactionService
{
    private readonly ApplicationState applicationState;

    public TransactionService(ApplicationState applicationState)
    {
        this.applicationState = applicationState;
    }

    public TransactionResult AddTransaction(int categoryId, decimal amount, string type, DateTime date, string? note)
    {
        if (!applicationState.IsLoggedIn)
        {
            return TransactionResult.Fail("You must login first.");
        }

        if (!TransactionTypes.IsValid(type))
        {
            return TransactionResult.Fail("Transaction type is invalid.");
        }

        if (amount <= 0)
        {
            return TransactionResult.Fail("Amount must be greater than zero.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var category = dbContext.Categories
            .AsNoTracking()
            .FirstOrDefault(category => category.CategoryId == categoryId && category.UserId == userId);

        if (category is null)
        {
            return TransactionResult.Fail("Category is required.");
        }

        if (category.Type != type)
        {
            return TransactionResult.Fail("Category type must match transaction type.");
        }

        var transaction = new Transaction
        {
            UserId = userId,
            CategoryId = category.CategoryId,
            Amount = amount,
            Type = type,
            Date = date.Date,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedAt = DateTime.Now
        };

        dbContext.Transactions.Add(transaction);
        dbContext.SaveChanges();

        return TransactionResult.Ok(transaction, "Transaction saved successfully.");
    }

    public Transaction? GetTransactionById(int transactionId)
    {
        if (!applicationState.IsLoggedIn)
        {
            return null;
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;

        return dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .FirstOrDefault(transaction => transaction.TransactionId == transactionId && transaction.UserId == userId);
    }

    public TransactionResult UpdateTransaction(int transactionId, int categoryId, decimal amount, string type, DateTime date, string? note)
    {
        if (!applicationState.IsLoggedIn)
        {
            return TransactionResult.Fail("You must login first.");
        }

        if (!TransactionTypes.IsValid(type))
        {
            return TransactionResult.Fail("Transaction type is invalid.");
        }

        if (amount <= 0)
        {
            return TransactionResult.Fail("Amount must be greater than zero.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var transaction = dbContext.Transactions
            .FirstOrDefault(transaction => transaction.TransactionId == transactionId && transaction.UserId == userId);

        if (transaction is null)
        {
            return TransactionResult.Fail("Transaction not found.");
        }

        var category = dbContext.Categories
            .AsNoTracking()
            .FirstOrDefault(category => category.CategoryId == categoryId && category.UserId == userId);

        if (category is null)
        {
            return TransactionResult.Fail("Category is required.");
        }

        if (category.Type != type)
        {
            return TransactionResult.Fail("Category type must match transaction type.");
        }

        transaction.CategoryId = category.CategoryId;
        transaction.Amount = amount;
        transaction.Type = type;
        transaction.Date = date.Date;
        transaction.Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        dbContext.SaveChanges();

        return TransactionResult.Ok(transaction, "Transaction updated successfully.");
    }

    public TransactionResult DeleteTransaction(int transactionId)
    {
        if (!applicationState.IsLoggedIn)
        {
            return TransactionResult.Fail("You must login first.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var transaction = dbContext.Transactions
            .FirstOrDefault(transaction => transaction.TransactionId == transactionId && transaction.UserId == userId);

        if (transaction is null)
        {
            return TransactionResult.Fail("Transaction not found.");
        }

        dbContext.Transactions.Remove(transaction);
        dbContext.SaveChanges();

        return TransactionResult.Ok(transaction, "Transaction deleted successfully.");
    }

    public List<Transaction> GetTransactionsForMonth(DateTime month)
    {
        if (!applicationState.IsLoggedIn)
        {
            return new List<Transaction>();
        }

        var startDate = new DateTime(month.Year, month.Month, 1);
        var endDate = startDate.AddMonths(1);

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;

        return dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Where(transaction =>
                transaction.UserId == userId
                && transaction.Date >= startDate
                && transaction.Date < endDate)
            .OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.CreatedAt)
            .ToList();
    }
}
