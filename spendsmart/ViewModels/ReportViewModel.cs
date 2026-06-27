using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using spendsmart.Services;

namespace spendsmart.ViewModels;

public class ReportViewModel : BaseViewModel
{
    private readonly ReportService reportService;
    private DateTime selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private decimal totalIncome;
    private decimal totalExpense;
    private bool showingExpense = true;
    private ISeries[] chartSeries = Array.Empty<ISeries>();

    public ReportViewModel(ReportService reportService)
    {
        this.reportService = reportService;
        CategoryItems = new ObservableCollection<ReportCategoryItemViewModel>();
        PreviousMonthCommand = new RelayCommand(() => ChangeMonth(-1));
        NextMonthCommand = new RelayCommand(() => ChangeMonth(1));
        ShowExpenseCommand = new RelayCommand(() => SelectReportType(true));
        ShowIncomeCommand = new RelayCommand(() => SelectReportType(false));
        RefreshCommand = new RelayCommand(Refresh);

        Refresh();
    }

    public ObservableCollection<ReportCategoryItemViewModel> CategoryItems { get; }

    public string MonthText
    {
        get
        {
            var lastDay = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);
            return $"{selectedMonth:MM/yyyy}  (01/{selectedMonth:MM}–{lastDay:00}/{selectedMonth:MM})";
        }
    }

    public string TotalIncomeText => FormatMoney(totalIncome, includeSign: true);

    public string TotalExpenseText => FormatMoney(-totalExpense);

    public string BalanceText => FormatMoney(totalIncome - totalExpense, includeSign: true);

    public bool IsExpenseSelected => showingExpense;

    public bool IsIncomeSelected => !showingExpense;

    public string EmptyMessage => CategoryItems.Count == 0 ? "Chưa có dữ liệu báo cáo trong tháng này." : string.Empty;

    public ISeries[] ChartSeries
    {
        get => chartSeries;
        private set => SetProperty(ref chartSeries, value);
    }

    public ICommand PreviousMonthCommand { get; }

    public ICommand NextMonthCommand { get; }

    public ICommand ShowExpenseCommand { get; }

    public ICommand ShowIncomeCommand { get; }

    public ICommand RefreshCommand { get; }

    public void Refresh()
    {
        var report = reportService.GetMonthlyReport(selectedMonth);
        totalIncome = report.TotalIncome;
        totalExpense = report.TotalExpense;

        var categoryItems = showingExpense
            ? report.ExpenseCategories
            : report.IncomeCategories;

        CategoryItems.Clear();
        foreach (var item in categoryItems)
        {
            CategoryItems.Add(new ReportCategoryItemViewModel(item));
        }

        ChartSeries = CategoryItems
            .Select(item => new PieSeries<decimal>
            {
                Name = item.CategoryName,
                Values = new[] { item.TotalAmount },
                InnerRadius = 58,
                Fill = new SolidColorPaint(ParseColor(item.Color)),
                Stroke = null
            })
            .Cast<ISeries>()
            .ToArray();

        OnPropertyChanged(nameof(TotalIncomeText));
        OnPropertyChanged(nameof(TotalExpenseText));
        OnPropertyChanged(nameof(BalanceText));
        OnPropertyChanged(nameof(EmptyMessage));
    }

    private void ChangeMonth(int offset)
    {
        selectedMonth = selectedMonth.AddMonths(offset);
        OnPropertyChanged(nameof(MonthText));
        Refresh();
    }

    private void SelectReportType(bool expense)
    {
        if (showingExpense == expense)
        {
            return;
        }

        showingExpense = expense;
        OnPropertyChanged(nameof(IsExpenseSelected));
        OnPropertyChanged(nameof(IsIncomeSelected));
        Refresh();
    }

    private static string FormatMoney(decimal amount, bool includeSign = false)
    {
        var prefix = includeSign && amount > 0 ? "+" : string.Empty;
        return $"{prefix}{amount:N0}đ";
    }

    private static SKColor ParseColor(string color)
    {
        try
        {
            return SKColor.Parse(color);
        }
        catch
        {
            return SKColors.Gray;
        }
    }
}

public sealed class ReportCategoryItemViewModel
{
    public ReportCategoryItemViewModel(CategoryReportItem item)
    {
        CategoryName = item.CategoryName;
        Icon = CategoryItemViewModel.GetIconSymbol(item.IconName);
        Color = item.Color;
        TotalAmount = item.TotalAmount;
        Percentage = item.Percentage;
    }

    public string CategoryName { get; }

    public string Icon { get; }

    public string Color { get; }

    public decimal TotalAmount { get; }

    public decimal Percentage { get; }

    public string TotalAmountText => $"{TotalAmount:N0}đ";

    public string PercentageText => $"{Percentage.ToString("0.0", CultureInfo.CurrentCulture)} %";
}
