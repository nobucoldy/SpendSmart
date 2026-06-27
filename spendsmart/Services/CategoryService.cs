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
}
