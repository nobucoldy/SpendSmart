using Microsoft.EntityFrameworkCore;
using spendsmart.Constants;
using spendsmart.Data;
using spendsmart.Models;

namespace spendsmart.Services;

public class TransactionService
{
    private const int NoteMaxLength = 255;
    private readonly ApplicationState applicationState;

    public TransactionService(ApplicationState applicationState)
    {
        this.applicationState = applicationState;
    }

    public TransactionResult AddTransaction(int categoryId, decimal amount, string type, DateTime date, string? note)
    {
        if (!applicationState.IsLoggedIn)
        {
            return TransactionResult.Fail("Bạn cần đăng nhập trước.");
        }

        if (!TransactionTypes.IsValid(type))
        {
            return TransactionResult.Fail("Loại giao dịch không hợp lệ.");
        }

        if (amount <= 0)
        {
            return TransactionResult.Fail("Số tiền phải lớn hơn 0.");
        }

        var normalizedNote = NormalizeNote(note);
        if (normalizedNote?.Length > NoteMaxLength)
        {
            return TransactionResult.Fail($"Ghi chú không được vượt quá {NoteMaxLength} ký tự.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var category = dbContext.Categories
            .AsNoTracking()
            .FirstOrDefault(category => category.CategoryId == categoryId && category.UserId == userId);

        if (category is null)
        {
            return TransactionResult.Fail("Vui lòng chọn danh mục.");
        }

        if (category.Type != type)
        {
            return TransactionResult.Fail("Loại danh mục phải khớp với loại giao dịch.");
        }

        var transaction = new Transaction
        {
            UserId = userId,
            CategoryId = category.CategoryId,
            Amount = amount,
            Type = type,
            Date = date.Date,
            Note = normalizedNote,
            CreatedAt = DateTime.Now
        };

        dbContext.Transactions.Add(transaction);
        dbContext.SaveChanges();

        return TransactionResult.Ok(transaction, "Lưu giao dịch thành công.");
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
            return TransactionResult.Fail("Bạn cần đăng nhập trước.");
        }

        if (!TransactionTypes.IsValid(type))
        {
            return TransactionResult.Fail("Loại giao dịch không hợp lệ.");
        }

        if (amount <= 0)
        {
            return TransactionResult.Fail("Số tiền phải lớn hơn 0.");
        }

        var normalizedNote = NormalizeNote(note);
        if (normalizedNote?.Length > NoteMaxLength)
        {
            return TransactionResult.Fail($"Ghi chú không được vượt quá {NoteMaxLength} ký tự.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var transaction = dbContext.Transactions
            .FirstOrDefault(transaction => transaction.TransactionId == transactionId && transaction.UserId == userId);

        if (transaction is null)
        {
            return TransactionResult.Fail("Không tìm thấy giao dịch.");
        }

        var category = dbContext.Categories
            .AsNoTracking()
            .FirstOrDefault(category => category.CategoryId == categoryId && category.UserId == userId);

        if (category is null)
        {
            return TransactionResult.Fail("Vui lòng chọn danh mục.");
        }

        if (category.Type != type)
        {
            return TransactionResult.Fail("Loại danh mục phải khớp với loại giao dịch.");
        }

        transaction.CategoryId = category.CategoryId;
        transaction.Amount = amount;
        transaction.Type = type;
        transaction.Date = date.Date;
        transaction.Note = normalizedNote;

        dbContext.SaveChanges();

        return TransactionResult.Ok(transaction, "Cập nhật giao dịch thành công.");
    }

    public TransactionResult DeleteTransaction(int transactionId)
    {
        if (!applicationState.IsLoggedIn)
        {
            return TransactionResult.Fail("Bạn cần đăng nhập trước.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var transaction = dbContext.Transactions
            .FirstOrDefault(transaction => transaction.TransactionId == transactionId && transaction.UserId == userId);

        if (transaction is null)
        {
            return TransactionResult.Fail("Không tìm thấy giao dịch.");
        }

        dbContext.Transactions.Remove(transaction);
        dbContext.SaveChanges();

        return TransactionResult.Ok(transaction, "Xóa giao dịch thành công.");
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

    private static string? NormalizeNote(string? note)
    {
        return string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}
