using spendsmart.Models;

namespace spendsmart.Services;

public sealed class CategoryResult
{
    private CategoryResult(bool success, string message, Category? category = null)
    {
        Success = success;
        Message = message;
        Category = category;
    }

    public bool Success { get; }

    public string Message { get; }

    public Category? Category { get; }

    public static CategoryResult Ok(Category category, string message)
    {
        return new CategoryResult(true, message, category);
    }

    public static CategoryResult Ok(string message)
    {
        return new CategoryResult(true, message);
    }

    public static CategoryResult Fail(string message)
    {
        return new CategoryResult(false, message);
    }
}
