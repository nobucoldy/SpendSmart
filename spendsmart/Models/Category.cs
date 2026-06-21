namespace spendsmart.Models;

public class Category
{
    public int CategoryId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string IconName { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public User? User { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
