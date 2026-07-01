using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using spendsmart.Constants;
using spendsmart.Models;
using spendsmart.Services;

namespace spendsmart.ViewModels;

public class CategoryManagementViewModel : BaseViewModel
{
    private readonly CategoryService categoryService;
    private string selectedType = TransactionTypes.Expense;
    private CategoryItemViewModel? selectedCategory;
    private string categoryName = string.Empty;
    private string iconName = "ShoppingBag";
    private string color = "#FF7043";
    private string statusMessage = string.Empty;
    private bool isEditing;
    private bool isFormOpen;

    public CategoryManagementViewModel(CategoryService categoryService)
    {
        this.categoryService = categoryService;
        Categories = new ObservableCollection<CategoryItemViewModel>();
        IconOptions = new[]
        {
            new CategoryIconOption("ShoppingBag", "🛒"),
            new CategoryIconOption("Taxi", "🚕"),
            new CategoryIconOption("Plane", "✈"),
            new CategoryIconOption("Food", "🍔"),
            new CategoryIconOption("Cake", "🍰"),
            new CategoryIconOption("IceCream", "🍦"),
            new CategoryIconOption("Bowl", "🍚"),
            new CategoryIconOption("Bread", "🍞"),
            new CategoryIconOption("Ship", "⛴"),
            new CategoryIconOption("Donut", "🍩"),
            new CategoryIconOption("Movie", "🎥"),
            new CategoryIconOption("Coffee", "☕"),
            new CategoryIconOption("Star", "☆"),
            new CategoryIconOption("Dress", "👗"),
            new CategoryIconOption("Shirt", "👖"),
            new CategoryIconOption("Wine", "🍷")
        };
        ColorOptions = new[]
        {
            "#FFE866", "#FFC0AD", "#FF8186", "#F7A8D5", "#F8B2F4",
            "#FF8100", "#FF1010", "#F64787", "#E44BA9", "#DD54F4",
            "#C67600", "#C00008", "#B63D52", "#C00078", "#A400B8",
            "#FFFF5B", "#DBFF4C", "#D8F8B0", "#8EEBD3", "#50DDE0"
        };

        SelectExpenseCommand = new RelayCommand(() => SelectType(TransactionTypes.Expense));
        SelectIncomeCommand = new RelayCommand(() => SelectType(TransactionTypes.Income));
        OpenCreateCommand = new RelayCommand(OpenCreateForm);
        BackCommand = new RelayCommand(CloseForm);
        SaveCommand = new RelayCommand(SaveCategory);
        DeleteCommand = new RelayCommand(DeleteCategory, () => SelectedCategory is not null);
        ClearFormCommand = new RelayCommand(OpenCreateForm);

        LoadCategories();
    }

    public ObservableCollection<CategoryItemViewModel> Categories { get; }

    public IReadOnlyList<CategoryIconOption> IconOptions { get; }

    public IReadOnlyList<string> ColorOptions { get; }

    public CategoryItemViewModel? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (SetProperty(ref selectedCategory, value))
            {
                LoadSelectedCategory();
                RelayCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string CategoryName
    {
        get => categoryName;
        set => SetProperty(ref categoryName, value);
    }

    public string IconName
    {
        get => iconName;
        set
        {
            if (SetProperty(ref iconName, value))
            {
                OnPropertyChanged(nameof(IconPreview));
            }
        }
    }

    public string Color
    {
        get => color;
        set => SetProperty(ref color, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    public bool IsExpenseSelected => selectedType == TransactionTypes.Expense;

    public bool IsIncomeSelected => selectedType == TransactionTypes.Income;

    public Visibility ListVisibility => isFormOpen ? Visibility.Collapsed : Visibility.Visible;

    public Visibility FormVisibility => isFormOpen ? Visibility.Visible : Visibility.Collapsed;

    public Visibility DeleteVisibility => isEditing ? Visibility.Visible : Visibility.Collapsed;

    public string PageTitle => isFormOpen ? (isEditing ? "Chỉnh sửa" : "Tạo mới") : "Danh mục";

    public string FormTitle => isEditing ? "Chỉnh sửa" : "Tạo mới";

    public string SaveButtonText => "Lưu";

    public string IconPreview => CategoryItemViewModel.GetIconSymbol(IconName);

    public ICommand SelectExpenseCommand { get; }

    public ICommand SelectIncomeCommand { get; }

    public ICommand OpenCreateCommand { get; }

    public ICommand BackCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand ClearFormCommand { get; }

    public void Refresh()
    {
        LoadCategories();
    }

    private void SelectType(string type)
    {
        selectedType = type;
        OnPropertyChanged(nameof(IsExpenseSelected));
        OnPropertyChanged(nameof(IsIncomeSelected));
        ResetForm();
        CloseForm();
        LoadCategories();
    }

    private void LoadCategories()
    {
        Categories.Clear();

        foreach (var category in categoryService.GetCategories(selectedType))
        {
            Categories.Add(new CategoryItemViewModel(category));
        }
    }

    private void LoadSelectedCategory()
    {
        if (SelectedCategory is null)
        {
            isEditing = false;
            OnFormModeChanged();
            return;
        }

        isEditing = true;
        CategoryName = SelectedCategory.Name;
        IconName = SelectedCategory.IconName;
        Color = SelectedCategory.Color;
        StatusMessage = string.Empty;
        isFormOpen = true;
        OnFormModeChanged();
    }

    private void SaveCategory()
    {
        StatusMessage = string.Empty;

        var result = isEditing && SelectedCategory is not null
            ? categoryService.UpdateCategory(SelectedCategory.CategoryId, CategoryName, IconName, Color)
            : categoryService.AddCategory(CategoryName, selectedType, IconName, Color);

        StatusMessage = result.Message;

        if (!result.Success)
        {
            return;
        }

        LoadCategories();
        ResetForm();
        CloseForm();
    }

    private void DeleteCategory()
    {
        if (SelectedCategory is null)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete category \"{SelectedCategory.Name}\"?",
            "Delete category",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var result = categoryService.DeleteCategory(SelectedCategory.CategoryId);
        StatusMessage = result.Message;

        if (result.Success)
        {
            LoadCategories();
            ResetForm();
            CloseForm();
        }
    }

    private void OpenCreateForm()
    {
        ResetForm();
        StatusMessage = string.Empty;
        isFormOpen = true;
        OnFormModeChanged();
    }

    private void CloseForm()
    {
        isFormOpen = false;
        OnFormModeChanged();
    }

    private void ResetForm()
    {
        SelectedCategory = null;
        isEditing = false;
        CategoryName = string.Empty;
        IconName = selectedType == TransactionTypes.Income ? "Wallet" : "ShoppingBag";
        Color = selectedType == TransactionTypes.Income ? "#26A69A" : "#FFE866";
        OnFormModeChanged();
    }

    private void OnFormModeChanged()
    {
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(IconPreview));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(ListVisibility));
        OnPropertyChanged(nameof(FormVisibility));
        OnPropertyChanged(nameof(DeleteVisibility));
        RelayCommand.RaiseCanExecuteChanged();
    }
}

public sealed class CategoryIconOption
{
    public CategoryIconOption(string iconName, string symbol)
    {
        IconName = iconName;
        Symbol = symbol;
    }

    public string IconName { get; }

    public string Symbol { get; }
}

public sealed class CategoryItemViewModel
{
    public CategoryItemViewModel(Category category)
    {
        CategoryId = category.CategoryId;
        Name = category.Name;
        Type = category.Type;
        IconName = category.IconName;
        Color = category.Color;
    }

    public int CategoryId { get; }

    public string Name { get; }

    public string Type { get; }

    public string IconName { get; }

    public string Color { get; }

    public string IconSymbol => GetIconSymbol(IconName);

    public static string GetIconSymbol(string iconName)
    {
        return iconName switch
        {
            "ShoppingBag" => "🛒",
            "Taxi" => "🚕",
            "Plane" => "✈",
            "Food" => "🍔",
            "Cake" => "🍰",
            "IceCream" => "🍦",
            "Bowl" => "🍚",
            "Bread" => "🍞",
            "Ship" => "⛴",
            "Donut" => "🍩",
            "Movie" => "🎥",
            "Coffee" => "☕",
            "Star" => "☆",
            "Dress" => "👗",
            "Shirt" => "👖",
            "Wine" => "🍷",
            "HeartPulse" => "💊",
            "BookOpen" => "📘",
            "Zap" => "⚡",
            "Bus" => "🚌",
            "Home" => "🏠",
            "Fuel" => "⛽",
            "Wallet" => "👛",
            "Gift" => "🎁",
            "Briefcase" => "💼",
            "TrendingUp" => "📈",
            "CircleDollar" => "💵",
            _ => "⋯"
        };
    }
}
