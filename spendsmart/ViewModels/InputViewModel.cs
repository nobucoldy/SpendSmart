using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using spendsmart.Constants;
using spendsmart.Services;

namespace spendsmart.ViewModels;

public class InputViewModel : BaseViewModel
{
    private readonly CategoryService categoryService;
    private readonly TransactionService transactionService;
    private string selectedType = TransactionTypes.Expense;
    private DateTime? selectedDate = DateTime.Today;
    private string note = string.Empty;
    private string amountText = string.Empty;
    private CategoryItemViewModel? selectedCategory;
    private string statusMessage = string.Empty;

    public InputViewModel(CategoryService categoryService, TransactionService transactionService)
    {
        this.categoryService = categoryService;
        this.transactionService = transactionService;

        Categories = new ObservableCollection<CategoryItemViewModel>();
        SelectExpenseCommand = new RelayCommand(() => SelectType(TransactionTypes.Expense));
        SelectIncomeCommand = new RelayCommand(() => SelectType(TransactionTypes.Income));
        SaveCommand = new RelayCommand(SaveTransaction);

        Refresh();
    }

    public ObservableCollection<CategoryItemViewModel> Categories { get; }

    public DateTime? SelectedDate
    {
        get => selectedDate;
        set
        {
            if (SetProperty(ref selectedDate, value))
            {
                OnPropertyChanged(nameof(SelectedDateText));
            }
        }
    }

    public string SelectedDateText => SelectedDate?.ToString("dd/MM/yyyy (ddd)", CultureInfo.CurrentCulture) ?? string.Empty;

    public string Note
    {
        get => note;
        set => SetProperty(ref note, value);
    }

    public string AmountText
    {
        get => amountText;
        set => SetProperty(ref amountText, value);
    }

    public CategoryItemViewModel? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (SetProperty(ref selectedCategory, value))
            {
                OnPropertyChanged(nameof(SelectedCategoryText));
            }
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    public bool IsExpenseSelected => selectedType == TransactionTypes.Expense;

    public bool IsIncomeSelected => selectedType == TransactionTypes.Income;

    public string AmountLabel => IsExpenseSelected ? "Tiền chi" : "Tiền thu";

    public string SaveButtonText => IsExpenseSelected ? "Nhập khoản chi" : "Nhập khoản thu";

    public string SelectedCategoryText => SelectedCategory is null
        ? "Chưa chọn danh mục"
        : $"Đã chọn: {SelectedCategory.Name}";

    public ICommand SelectExpenseCommand { get; }

    public ICommand SelectIncomeCommand { get; }

    public ICommand SaveCommand { get; }

    public void Refresh()
    {
        LoadCategories();
    }

    private void SelectType(string type)
    {
        selectedType = type;
        OnPropertyChanged(nameof(IsExpenseSelected));
        OnPropertyChanged(nameof(IsIncomeSelected));
        OnPropertyChanged(nameof(AmountLabel));
        OnPropertyChanged(nameof(SaveButtonText));
        LoadCategories();
    }

    private void LoadCategories()
    {
        Categories.Clear();

        foreach (var category in categoryService.GetCategories(selectedType))
        {
            Categories.Add(new CategoryItemViewModel(category));
        }

        SelectedCategory = Categories.FirstOrDefault();

        if (Categories.Count == 0)
        {
            StatusMessage = "No categories found. Add a category first.";
        }
        else if (StatusMessage == "No categories found. Add a category first.")
        {
            StatusMessage = string.Empty;
        }
    }

    private void SaveTransaction()
    {
        StatusMessage = string.Empty;

        if (SelectedCategory is null)
        {
            StatusMessage = "Category is required.";
            return;
        }

        if (!SelectedDate.HasValue)
        {
            StatusMessage = "Date is required.";
            return;
        }

        if (!decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.CurrentCulture, out var amount)
            && !decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
        {
            StatusMessage = "Amount must be a valid number.";
            return;
        }

        var result = transactionService.AddTransaction(
            SelectedCategory.CategoryId,
            amount,
            selectedType,
            SelectedDate.Value,
            Note);

        StatusMessage = result.Message;

        if (!result.Success)
        {
            return;
        }

        AmountText = string.Empty;
        Note = string.Empty;
        SelectedDate = DateTime.Today;
        LoadCategories();
    }
}
