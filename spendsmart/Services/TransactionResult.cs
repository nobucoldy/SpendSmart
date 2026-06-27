using spendsmart.Models;

namespace spendsmart.Services;

public sealed class TransactionResult
{
    private TransactionResult(bool success, string message, Transaction? transaction = null)
    {
        Success = success;
        Message = message;
        Transaction = transaction;
    }

    public bool Success { get; }

    public string Message { get; }

    public Transaction? Transaction { get; }

    public static TransactionResult Ok(Transaction transaction, string message)
    {
        return new TransactionResult(true, message, transaction);
    }

    public static TransactionResult Fail(string message)
    {
        return new TransactionResult(false, message);
    }
}
