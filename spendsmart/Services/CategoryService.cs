using Microsoft.EntityFrameworkCore;
using spendsmart.Constants;
using spendsmart.Data;
using spendsmart.Models;

namespace spendsmart.Services;

public class CategoryService
{
    private readonly ApplicationState applicationState;

    public CategoryService(ApplicationState applicationState)
    {
        this.applicationState = applicationState;
    }

    public List<Category> GetCategories(string type)
    {
        if (!applicationState.IsLoggedIn || !TransactionTypes.IsValid(type))
        {
            return new List<Category>();
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;

        RenameLegacyDefaultCategories(dbContext, userId);

        var categories = dbContext.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId && category.Type == type)
            .OrderBy(category => category.Name)
            .ToList();

        if (categories.Count > 0)
        {
            return categories;
        }

        SeedDefaultCategoriesForType(dbContext, userId, type);

        return dbContext.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId && category.Type == type)
            .OrderBy(category => category.Name)
            .ToList();
    }

    public CategoryResult AddCategory(string name, string type, string iconName, string color)
    {
        if (!applicationState.IsLoggedIn)
        {
            return CategoryResult.Fail("You must login first.");
        }

        var validationError = ValidateCategory(name, type, iconName, color);
        if (validationError is not null)
        {
            return CategoryResult.Fail(validationError);
        }

        name = name.Trim();
        iconName = iconName.Trim();
        color = color.Trim();

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;

        if (CategoryNameExists(dbContext, userId, type, name))
        {
            return CategoryResult.Fail("Category name already exists for this type.");
        }

        var category = new Category
        {
            UserId = userId,
            Name = name,
            Type = type,
            IconName = iconName,
            Color = color
        };

        dbContext.Categories.Add(category);
        dbContext.SaveChanges();

        return CategoryResult.Ok(category, "Category added successfully.");
    }

    public CategoryResult UpdateCategory(int categoryId, string name, string iconName, string color)
    {
        if (!applicationState.IsLoggedIn)
        {
            return CategoryResult.Fail("You must login first.");
        }

        var validationError = ValidateCategory(name, TransactionTypes.Expense, iconName, color, validateType: false);
        if (validationError is not null)
        {
            return CategoryResult.Fail(validationError);
        }

        name = name.Trim();
        iconName = iconName.Trim();
        color = color.Trim();

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var category = dbContext.Categories.FirstOrDefault(category =>
            category.CategoryId == categoryId && category.UserId == userId);

        if (category is null)
        {
            return CategoryResult.Fail("Category was not found.");
        }

        if (CategoryNameExists(dbContext, userId, category.Type, name, category.CategoryId))
        {
            return CategoryResult.Fail("Category name already exists for this type.");
        }

        category.Name = name;
        category.IconName = iconName;
        category.Color = color;

        dbContext.SaveChanges();
        return CategoryResult.Ok(category, "Category updated successfully.");
    }

    public CategoryResult ChangeCategoryType(int categoryId, string newType)
    {
        if (!applicationState.IsLoggedIn)
        {
            return CategoryResult.Fail("You must login first.");
        }

        if (!TransactionTypes.IsValid(newType))
        {
            return CategoryResult.Fail("Category type is invalid.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var category = dbContext.Categories.FirstOrDefault(category =>
            category.CategoryId == categoryId && category.UserId == userId);

        if (category is null)
        {
            return CategoryResult.Fail("Category was not found.");
        }

        if (dbContext.Transactions.Any(transaction => transaction.CategoryId == category.CategoryId))
        {
            return CategoryResult.Fail("Cannot change category type because it already has transactions.");
        }

        if (CategoryNameExists(dbContext, userId, newType, category.Name, category.CategoryId))
        {
            return CategoryResult.Fail("Category name already exists for this type.");
        }

        category.Type = newType;
        dbContext.SaveChanges();

        return CategoryResult.Ok(category, "Category type updated successfully.");
    }

    public CategoryResult DeleteCategory(int categoryId)
    {
        if (!applicationState.IsLoggedIn)
        {
            return CategoryResult.Fail("You must login first.");
        }

        using var dbContext = new AppDbContext();
        var userId = applicationState.CurrentUser!.UserId;
        var category = dbContext.Categories.FirstOrDefault(category =>
            category.CategoryId == categoryId && category.UserId == userId);

        if (category is null)
        {
            return CategoryResult.Fail("Category was not found.");
        }

        if (dbContext.Transactions.Any(transaction => transaction.CategoryId == category.CategoryId))
        {
            return CategoryResult.Fail("Cannot delete category because it already has transactions.");
        }

        dbContext.Categories.Remove(category);
        dbContext.SaveChanges();

        return CategoryResult.Ok("Category deleted successfully.");
    }

    private static bool CategoryNameExists(
        AppDbContext dbContext,
        int userId,
        string type,
        string name,
        int? excludedCategoryId = null)
    {
        return dbContext.Categories.Any(category =>
            category.UserId == userId
            && category.Type == type
            && category.Name == name
            && (!excludedCategoryId.HasValue || category.CategoryId != excludedCategoryId.Value));
    }

    private static string? ValidateCategory(
        string name,
        string type,
        string iconName,
        string color,
        bool validateType = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Category name is required.";
        }

        if (validateType && !TransactionTypes.IsValid(type))
        {
            return "Category type is invalid.";
        }

        if (string.IsNullOrWhiteSpace(iconName))
        {
            return "Icon name is required.";
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            return "Color is required.";
        }

        if (!color.Trim().StartsWith("#", StringComparison.Ordinal) || color.Trim().Length != 7)
        {
            return "Color must use format #RRGGBB.";
        }

        return null;
    }

    private static void SeedDefaultCategoriesForType(AppDbContext dbContext, int userId, string type)
    {
        var defaults = type == TransactionTypes.Income
            ? CreateDefaultIncomeCategories(userId)
            : CreateDefaultExpenseCategories(userId);

        foreach (var category in defaults)
        {
            var exists = dbContext.Categories.Any(existing =>
                existing.UserId == userId
                && existing.Type == category.Type
                && existing.Name == category.Name);

            if (!exists)
            {
                dbContext.Categories.Add(category);
            }
        }

        dbContext.SaveChanges();
    }

    private static void RenameLegacyDefaultCategories(AppDbContext dbContext, int userId)
    {
        var renamed = false;
        var legacyNames = new Dictionary<string, string>
        {
            ["Food"] = "Ăn uống",
            ["Daily Expenses"] = "Sinh hoạt",
            ["Clothes"] = "Quần áo",
            ["Healthcare"] = "Sức khỏe",
            ["Education"] = "Giáo dục",
            ["Electricity"] = "Tiền điện",
            ["Transportation"] = "Đi lại",
            ["Rent"] = "Thuê nhà",
            ["Fuel"] = "Xăng dầu",
            ["Miscellaneous"] = "Khác",
            ["Salary"] = "Lương",
            ["Bonus"] = "Thưởng",
            ["Business"] = "Kinh doanh",
            ["Investment"] = "Đầu tư",
            ["Other Income"] = "Thu nhập khác"
        };

        var categories = dbContext.Categories
            .Where(category => category.UserId == userId && legacyNames.Keys.Contains(category.Name))
            .ToList();

        foreach (var category in categories)
        {
            var newName = legacyNames[category.Name];
            var duplicateExists = dbContext.Categories.Any(existing =>
                existing.UserId == userId
                && existing.Type == category.Type
                && existing.Name == newName
                && existing.CategoryId != category.CategoryId);

            if (duplicateExists)
            {
                continue;
            }

            category.Name = newName;
            renamed = true;
        }

        if (renamed)
        {
            dbContext.SaveChanges();
        }
    }

    private static List<Category> CreateDefaultExpenseCategories(int userId)
    {
        return new List<Category>
        {
            new() { UserId = userId, Name = "Ăn uống", Type = TransactionTypes.Expense, IconName = "Food", Color = "#FF7043" },
            new() { UserId = userId, Name = "Sinh hoạt", Type = TransactionTypes.Expense, IconName = "ShoppingBag", Color = "#42A5F5" },
            new() { UserId = userId, Name = "Quần áo", Type = TransactionTypes.Expense, IconName = "Shirt", Color = "#AB47BC" },
            new() { UserId = userId, Name = "Sức khỏe", Type = TransactionTypes.Expense, IconName = "HeartPulse", Color = "#EF5350" },
            new() { UserId = userId, Name = "Giáo dục", Type = TransactionTypes.Expense, IconName = "BookOpen", Color = "#5C6BC0" },
            new() { UserId = userId, Name = "Tiền điện", Type = TransactionTypes.Expense, IconName = "Zap", Color = "#FFA726" },
            new() { UserId = userId, Name = "Đi lại", Type = TransactionTypes.Expense, IconName = "Bus", Color = "#26A69A" },
            new() { UserId = userId, Name = "Thuê nhà", Type = TransactionTypes.Expense, IconName = "Home", Color = "#8D6E63" },
            new() { UserId = userId, Name = "Xăng dầu", Type = TransactionTypes.Expense, IconName = "Fuel", Color = "#78909C" },
            new() { UserId = userId, Name = "Khác", Type = TransactionTypes.Expense, IconName = "MoreHorizontal", Color = "#66BB6A" }
        };
    }

    private static List<Category> CreateDefaultIncomeCategories(int userId)
    {
        return new List<Category>
        {
            new() { UserId = userId, Name = "Lương", Type = TransactionTypes.Income, IconName = "Wallet", Color = "#26A69A" },
            new() { UserId = userId, Name = "Thưởng", Type = TransactionTypes.Income, IconName = "Gift", Color = "#7E57C2" },
            new() { UserId = userId, Name = "Kinh doanh", Type = TransactionTypes.Income, IconName = "Briefcase", Color = "#5C6BC0" },
            new() { UserId = userId, Name = "Đầu tư", Type = TransactionTypes.Income, IconName = "TrendingUp", Color = "#FFCA28" },
            new() { UserId = userId, Name = "Thu nhập khác", Type = TransactionTypes.Income, IconName = "CircleDollar", Color = "#66BB6A" }
        };
    }
}
