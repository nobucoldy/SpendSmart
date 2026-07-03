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
    private readonly Action? editCompleted;
    private readonly Func<bool>? confirmDelete;
    private int? editingTransactionId;
    private string selectedType = TransactionTypes.Expense;
    private DateTime? selectedDate = DateTime.Today;
    private string note = string.Empty;
    private string amountText = string.Empty;
    private CategoryItemViewModel? selectedCategory;
    private string statusMessage = string.Empty;

    public InputViewModel(
        CategoryService categoryService,
        TransactionService transactionService,
        Action? editCompleted = null,
        Func<bool>? confirmDelete = null)
    {
        this.categoryService = categoryService;
        this.transactionService = transactionService;
        this.editCompleted = editCompleted;
        this.confirmDelete = confirmDelete;

        Categories = new ObservableCollection<CategoryItemViewModel>();
        SelectExpenseCommand = new RelayCommand(() => SelectType(TransactionTypes.Expense));
        SelectIncomeCommand = new RelayCommand(() => SelectType(TransactionTypes.Income));
        PreviousDateCommand = new RelayCommand(() => ChangeDate(-1));
        NextDateCommand = new RelayCommand(() => ChangeDate(1));
        SaveCommand = new RelayCommand(SaveTransaction);
        DeleteCommand = new RelayCommand(DeleteTransaction, () => IsEditMode);
        NewTransactionCommand = new RelayCommand(StartNewTransaction);

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

    public string SelectedDateText => SelectedDate.HasValue
        ? $"{SelectedDate.Value:dd/MM/yyyy} ({GetVietnameseDayName(SelectedDate.Value)})"
        : string.Empty;

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

    public bool IsEditMode => editingTransactionId.HasValue;

    public bool IsCreateMode => !IsEditMode;

    public string PageTitle => IsEditMode ? "Chỉnh sửa khoản thu chi" : string.Empty;

    public string AmountLabel => IsExpenseSelected ? "Tiền chi" : "Tiền thu";

    public string SaveButtonText
    {
        get
        {
            if (IsEditMode)
            {
                return "Cập nhật khoản thu chi";
            }

            return IsExpenseSelected ? "Nhập khoản chi" : "Nhập khoản thu";
        }
    }

    public string SelectedCategoryText => SelectedCategory is null
        ? "Chưa chọn danh mục"
        : $"Đã chọn: {SelectedCategory.Name}";

    public ICommand SelectExpenseCommand { get; }

    public ICommand SelectIncomeCommand { get; }

    public ICommand PreviousDateCommand { get; }

    public ICommand NextDateCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand NewTransactionCommand { get; }

    public void Refresh()
    {
        if (IsEditMode)
        {
            return;
        }

        LoadCategories();
    }

    public void LoadTransactionForEdit(int transactionId)
    {
        var transaction = transactionService.GetTransactionById(transactionId);
        if (transaction is null)
        {
            StatusMessage = "Transaction not found.";
            return;
        }

        editingTransactionId = transaction.TransactionId;
        selectedType = transaction.Type;
        SelectedDate = transaction.Date;
        Note = transaction.Note ?? string.Empty;
        AmountText = transaction.Amount.ToString("0.##", CultureInfo.CurrentCulture);
        LoadCategories(transaction.CategoryId);
        StatusMessage = string.Empty;
        NotifyModeChanged();
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

    private void ChangeDate(int offset)
    {
        SelectedDate = (SelectedDate ?? DateTime.Today).AddDays(offset);
    }

    private void LoadCategories(int? selectedCategoryId = null)
    {
        Categories.Clear();

        foreach (var category in categoryService.GetCategories(selectedType))
        {
            Categories.Add(new CategoryItemViewModel(category));
        }

        SelectedCategory = selectedCategoryId.HasValue
            ? Categories.FirstOrDefault(category => category.CategoryId == selectedCategoryId.Value) ?? Categories.FirstOrDefault()
            : Categories.FirstOrDefault();

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

        var result = IsEditMode
            ? transactionService.UpdateTransaction(
                editingTransactionId!.Value,
                SelectedCategory.CategoryId,
                amount,
                selectedType,
                SelectedDate.Value,
                Note)
            : transactionService.AddTransaction(
                SelectedCategory.CategoryId,
                amount,
                selectedType,
                SelectedDate.Value,
                Note);

        if (!result.Success)
        {
            StatusMessage = result.Message;
            return;
        }

        if (IsEditMode)
        {
            StartNewTransaction();
            editCompleted?.Invoke();
            return;
        }

        AmountText = string.Empty;
        Note = string.Empty;
        SelectedDate = DateTime.Today;
        StatusMessage = string.Empty;
        LoadCategories();
    }

    private void DeleteTransaction()
    {
        if (!editingTransactionId.HasValue)
        {
            return;
        }

        if (confirmDelete is not null && !confirmDelete())
        {
            return;
        }

        var result = transactionService.DeleteTransaction(editingTransactionId.Value);

        if (result.Success)
        {
            StartNewTransaction();
            editCompleted?.Invoke();
            return;
        }

        StatusMessage = result.Message;
    }

    private void StartNewTransaction()
    {
        editingTransactionId = null;
        selectedType = TransactionTypes.Expense;
        SelectedDate = DateTime.Today;
        Note = string.Empty;
        AmountText = string.Empty;
        LoadCategories();
        NotifyModeChanged();
    }

    private void NotifyModeChanged()
    {
        OnPropertyChanged(nameof(IsExpenseSelected));
        OnPropertyChanged(nameof(IsIncomeSelected));
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(IsCreateMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(AmountLabel));
        OnPropertyChanged(nameof(SaveButtonText));
        RelayCommand.RaiseCanExecuteChanged();
    }

    private static string GetVietnameseDayName(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Monday => "Thứ hai",
            DayOfWeek.Tuesday => "Thứ ba",
            DayOfWeek.Wednesday => "Thứ tư",
            DayOfWeek.Thursday => "Thứ năm",
            DayOfWeek.Friday => "Thứ sáu",
            DayOfWeek.Saturday => "Thứ bảy",
            DayOfWeek.Sunday => "Chủ nhật",
            _ => string.Empty
        };
    }
}
