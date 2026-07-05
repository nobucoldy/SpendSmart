using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using spendsmart.Constants;
using spendsmart.Services;
using spendsmart.Views;

namespace spendsmart.ViewModels;

public class BudgetViewModel : BaseViewModel
{
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

    private readonly BudgetService budgetService;
    private readonly CategoryService categoryService;
    private DateTime selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private BudgetCategoryOption? selectedCategoryOption;
    private string limitAmountText = string.Empty;
    private string statusMessage = string.Empty;
    private decimal predictedNextMonthExpense;

    public BudgetViewModel(BudgetService budgetService, CategoryService categoryService)
    {
        this.budgetService = budgetService;
        this.categoryService = categoryService;

        Budgets = new ObservableCollection<BudgetStatus>();
        CategoryOptions = new ObservableCollection<BudgetCategoryOption>();

        PreviousMonthCommand = new RelayCommand(() => ChangeMonth(-1));
        NextMonthCommand = new RelayCommand(() => ChangeMonth(1));
        SaveCommand = new RelayCommand(SaveBudget);
        DeleteCommand = new RelayCommand(parameter => DeleteBudget(parameter as BudgetStatus));

        Refresh();
    }

    public ObservableCollection<BudgetStatus> Budgets { get; }

    public ObservableCollection<BudgetCategoryOption> CategoryOptions { get; }

    public string MonthText => $"Tháng {selectedMonth.Month:00}/{selectedMonth.Year}";

    public BudgetCategoryOption? SelectedCategoryOption
    {
        get => selectedCategoryOption;
        set => SetProperty(ref selectedCategoryOption, value);
    }

    public string LimitAmountText
    {
        get => limitAmountText;
        set => SetProperty(ref limitAmountText, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    public decimal PredictedNextMonthExpense
    {
        get => predictedNextMonthExpense;
        private set
        {
            if (SetProperty(ref predictedNextMonthExpense, value))
            {
                OnPropertyChanged(nameof(PredictedNextMonthExpenseText));
            }
        }
    }

    public string PredictedNextMonthExpenseText => PredictedNextMonthExpense > 0
        ? $"Dự đoán chi tiêu tháng tới: {PredictedNextMonthExpense.ToString("N0", VietnameseCulture)} đ (trung bình 3 tháng gần nhất)"
        : "Chưa đủ dữ liệu lịch sử để dự đoán chi tiêu tháng tới.";

    public ICommand PreviousMonthCommand { get; }

    public ICommand NextMonthCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public void Refresh()
    {
        LoadCategoryOptions();
        LoadBudgets();
        PredictedNextMonthExpense = budgetService.PredictNextMonthExpense();
    }

    private void ChangeMonth(int offset)
    {
        selectedMonth = selectedMonth.AddMonths(offset);
        OnPropertyChanged(nameof(MonthText));
        LoadBudgets();
    }

    private void LoadCategoryOptions()
    {
        var previousSelectionCategoryId = SelectedCategoryOption?.CategoryId;

        CategoryOptions.Clear();
        CategoryOptions.Add(new BudgetCategoryOption(null, "Tổng chi tiêu (tất cả danh mục)"));

        foreach (var category in categoryService.GetCategories(TransactionTypes.Expense))
        {
            CategoryOptions.Add(new BudgetCategoryOption(category.CategoryId, category.Name));
        }

        SelectedCategoryOption = CategoryOptions.FirstOrDefault(option =>
            option.CategoryId == previousSelectionCategoryId) ?? CategoryOptions.FirstOrDefault();
    }

    private void LoadBudgets()
    {
        Budgets.Clear();

        foreach (var status in budgetService.GetBudgetStatuses(selectedMonth.Year, selectedMonth.Month))
        {
            Budgets.Add(status);
        }
    }

    private void SaveBudget()
    {
        StatusMessage = string.Empty;

        if (!decimal.TryParse(LimitAmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var limitAmount))
        {
            StatusMessage = "Hạn mức không hợp lệ.";
            return;
        }

        var result = budgetService.SetBudget(
            SelectedCategoryOption?.CategoryId,
            selectedMonth.Year,
            selectedMonth.Month,
            limitAmount);

        StatusMessage = result.Message;

        if (result.Success)
        {
            LimitAmountText = string.Empty;
            LoadBudgets();
        }
    }

    private void DeleteBudget(BudgetStatus? budget)
    {
        if (budget is null)
        {
            return;
        }

        var activeWindow = Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive);

        var dialog = new ConfirmDialog(
            "Xóa ngân sách",
            $"Bạn có chắc chắn muốn xóa ngân sách \"{budget.CategoryName}\" không?",
            "Xóa",
            "Hủy");

        if (activeWindow is not null)
        {
            dialog.Owner = activeWindow;
        }

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var result = budgetService.DeleteBudget(budget.BudgetId);
        StatusMessage = result.Message;

        if (result.Success)
        {
            LoadBudgets();
        }
    }
}

public sealed class BudgetCategoryOption
{
    public BudgetCategoryOption(int? categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }

    public int? CategoryId { get; }

    public string Name { get; }
}
