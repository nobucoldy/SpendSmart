using Microsoft.EntityFrameworkCore;
using spendsmart.Constants;
using spendsmart.Data;
using spendsmart.Models;

namespace spendsmart.Services;

public class AuthService
{
    private readonly ApplicationState applicationState;

    public AuthService(ApplicationState applicationState)
    {
        this.applicationState = applicationState;
    }

    public AuthResult Register(string fullName, string email, string password, string confirmPassword)
    {
        fullName = fullName.Trim();
        email = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return AuthResult.Fail("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthResult.Fail("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Fail("Password is required.");
        }

        if (password != confirmPassword)
        {
            return AuthResult.Fail("Confirm password does not match.");
        }

        using var dbContext = new AppDbContext();

        if (dbContext.Users.Any(user => user.Email == email))
        {
            return AuthResult.Fail("Email already exists.");
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Password = password,
            CreatedAt = DateTime.Now
        };

        user.Categories = CreateDefaultCategories();

        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        applicationState.SetCurrentUser(user);
        return AuthResult.Ok(user, "Register successfully.");
    }

    public AuthResult Login(string email, string password)
    {
        email = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthResult.Fail("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Fail("Password is required.");
        }

        using var dbContext = new AppDbContext();

        var user = dbContext.Users
            .AsNoTracking()
            .FirstOrDefault(user => user.Email == email && user.Password == password);

        if (user is null)
        {
            return AuthResult.Fail("Email or password is incorrect.");
        }

        applicationState.SetCurrentUser(user);
        return AuthResult.Ok(user, "Login successfully.");
    }

    public void Logout()
    {
        applicationState.ClearCurrentUser();
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static List<Category> CreateDefaultCategories()
    {
        return new List<Category>
        {
            new() { Name = "Food", Type = TransactionTypes.Expense, IconName = "Food", Color = "#FF7043" },
            new() { Name = "Daily Expenses", Type = TransactionTypes.Expense, IconName = "ShoppingBag", Color = "#42A5F5" },
            new() { Name = "Clothes", Type = TransactionTypes.Expense, IconName = "Shirt", Color = "#AB47BC" },
            new() { Name = "Healthcare", Type = TransactionTypes.Expense, IconName = "HeartPulse", Color = "#EF5350" },
            new() { Name = "Education", Type = TransactionTypes.Expense, IconName = "BookOpen", Color = "#5C6BC0" },
            new() { Name = "Electricity", Type = TransactionTypes.Expense, IconName = "Zap", Color = "#FFA726" },
            new() { Name = "Transportation", Type = TransactionTypes.Expense, IconName = "Bus", Color = "#26A69A" },
            new() { Name = "Rent", Type = TransactionTypes.Expense, IconName = "Home", Color = "#8D6E63" },
            new() { Name = "Fuel", Type = TransactionTypes.Expense, IconName = "Fuel", Color = "#78909C" },
            new() { Name = "Miscellaneous", Type = TransactionTypes.Expense, IconName = "MoreHorizontal", Color = "#66BB6A" },
            new() { Name = "Salary", Type = TransactionTypes.Income, IconName = "Wallet", Color = "#26A69A" },
            new() { Name = "Bonus", Type = TransactionTypes.Income, IconName = "Gift", Color = "#7E57C2" },
            new() { Name = "Business", Type = TransactionTypes.Income, IconName = "Briefcase", Color = "#5C6BC0" },
            new() { Name = "Investment", Type = TransactionTypes.Income, IconName = "TrendingUp", Color = "#FFCA28" },
            new() { Name = "Other Income", Type = TransactionTypes.Income, IconName = "CircleDollar", Color = "#66BB6A" }
        };
    }
}
