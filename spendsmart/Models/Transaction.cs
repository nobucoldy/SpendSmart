namespace spendsmart.Models;

public class Transaction
{
    public int TransactionId { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Today;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User? User { get; set; }

    public Category? Category { get; set; }
}
