namespace spendsmart.Models;

public class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
