namespace spendsmart.Models;

public class Budget
{
    public int BudgetId { get; set; }

    public int UserId { get; set; }

    public int? CategoryId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal LimitAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User? User { get; set; }

    public Category? Category { get; set; }
}
