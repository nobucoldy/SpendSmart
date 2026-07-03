using System.Collections.ObjectModel;
using spendsmart.Constants;
using spendsmart.Models;
using spendsmart.Services;

namespace spendsmart.ViewModels;

public class HistoryViewModel : BaseViewModel
{
    private readonly TransactionService transactionService;
    private readonly Action<int>? editTransaction;
    private DateTime selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime? selectedDate = DateTime.Today;
    private decimal totalIncome;
    private decimal totalExpense;
    private decimal balance;

    public HistoryViewModel(TransactionService transactionService, Action<int>? editTransaction = null)
    {
        this.transactionService = transactionService;
        this.editTransaction = editTransaction;
        Transactions = new ObservableCollection<HistoryListItemViewModel>();
        PreviousMonthCommand = new RelayCommand(() => ChangeMonth(-1));
        NextMonthCommand = new RelayCommand(() => ChangeMonth(1));
        RefreshCommand = new RelayCommand(Refresh);
        EditTransactionCommand = new RelayCommand(EditTransaction);

        Refresh();
    }

    public ObservableCollection<HistoryListItemViewModel> Transactions { get; }

    public DateTime? SelectedDate
    {
        get => selectedDate;
        set
        {
            if (!SetProperty(ref selectedDate, value) || !selectedDate.HasValue)
            {
                return;
            }

            var month = new DateTime(selectedDate.Value.Year, selectedDate.Value.Month, 1);
            if (month != selectedMonth)
            {
                selectedMonth = month;
                OnPropertyChanged(nameof(MonthText));
                Refresh();
            }
        }
    }

    public string MonthText
    {
        get
        {
            var lastDay = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);
            return $"{selectedMonth:MM/yyyy}  (01/{selectedMonth:MM}-{lastDay:00}/{selectedMonth:MM})";
        }
    }

    public string TotalIncomeText => FormatMoney(totalIncome);

    public string TotalExpenseText => FormatMoney(totalExpense);

    public string BalanceText => FormatMoney(balance, includeSign: true);

    public string EmptyMessage => Transactions.Count == 0 ? "Chưa có giao dịch trong tháng này." : string.Empty;

    public System.Windows.Input.ICommand PreviousMonthCommand { get; }

    public System.Windows.Input.ICommand NextMonthCommand { get; }

    public System.Windows.Input.ICommand RefreshCommand { get; }

    public System.Windows.Input.ICommand EditTransactionCommand { get; }

    public void Refresh()
    {
        var transactions = transactionService.GetTransactionsForMonth(selectedMonth);

        totalIncome = transactions
            .Where(transaction => transaction.Type == TransactionTypes.Income)
            .Sum(transaction => transaction.Amount);

        totalExpense = transactions
            .Where(transaction => transaction.Type == TransactionTypes.Expense)
            .Sum(transaction => transaction.Amount);

        balance = totalIncome - totalExpense;

        Transactions.Clear();
        foreach (var group in transactions.GroupBy(transaction => transaction.Date.Date).OrderByDescending(group => group.Key))
        {
            var dailyIncome = group
                .Where(transaction => transaction.Type == TransactionTypes.Income)
                .Sum(transaction => transaction.Amount);

            var dailyExpense = group
                .Where(transaction => transaction.Type == TransactionTypes.Expense)
                .Sum(transaction => transaction.Amount);

            Transactions.Add(HistoryListItemViewModel.CreateHeader(group.Key, dailyIncome - dailyExpense));

            foreach (var transaction in group)
            {
                Transactions.Add(HistoryListItemViewModel.CreateTransaction(transaction));
            }
        }

        OnPropertyChanged(nameof(TotalIncomeText));
        OnPropertyChanged(nameof(TotalExpenseText));
        OnPropertyChanged(nameof(BalanceText));
        OnPropertyChanged(nameof(EmptyMessage));
    }

    private void ChangeMonth(int offset)
    {
        selectedMonth = selectedMonth.AddMonths(offset);
        selectedDate = selectedMonth;
        OnPropertyChanged(nameof(SelectedDate));
        OnPropertyChanged(nameof(MonthText));
        Refresh();
    }

    private void EditTransaction(object? parameter)
    {
        if (parameter is int transactionId)
        {
            editTransaction?.Invoke(transactionId);
        }
    }

    private static string FormatMoney(decimal amount, bool includeSign = false)
    {
        var prefix = includeSign && amount > 0 ? "+" : string.Empty;
        return $"{prefix}{amount:N0}đ";
    }
}

public sealed class HistoryListItemViewModel
{
    private HistoryListItemViewModel()
    {
    }

    public bool IsHeader { get; private init; }

    public bool IsTransaction => !IsHeader;

    public string HeaderDateText { get; private init; } = string.Empty;

    public string HeaderAmountText { get; private init; } = string.Empty;

    public string HeaderAmountColor { get; private init; } = "#555555";

    public TransactionHistoryItemViewModel? Transaction { get; private init; }

    public static HistoryListItemViewModel CreateHeader(DateTime date, decimal total)
    {
        return new HistoryListItemViewModel
        {
            IsHeader = true,
            HeaderDateText = $"{date:dd/MM/yyyy} ({GetVietnameseDayName(date)})",
            HeaderAmountText = FormatSignedMoney(total),
            HeaderAmountColor = total >= 0 ? "#419CDF" : "#F05A3A"
        };
    }

    public static HistoryListItemViewModel CreateTransaction(Transaction transaction)
    {
        return new HistoryListItemViewModel
        {
            Transaction = new TransactionHistoryItemViewModel(transaction)
        };
    }

    private static string FormatSignedMoney(decimal amount)
    {
        if (amount == 0)
        {
            return "0đ";
        }

        var sign = amount > 0 ? "+" : "-";
        return $"{sign}{Math.Abs(amount):N0}đ";
    }

    private static string GetVietnameseDayName(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            DayOfWeek.Sunday => "CN",
            _ => string.Empty
        };
    }
}

public sealed class TransactionHistoryItemViewModel
{
    public TransactionHistoryItemViewModel(Transaction transaction)
    {
        TransactionId = transaction.TransactionId;
        CategoryName = transaction.Category?.Name ?? "Unknown";
        CategoryIcon = CategoryItemViewModel.GetIconSymbol(transaction.Category?.IconName ?? string.Empty);
        CategoryColor = transaction.Category?.Color ?? "#666666";
        Amount = transaction.Amount;
        Type = transaction.Type;
        Date = transaction.Date;
        Note = transaction.Note ?? string.Empty;
    }

    public int TransactionId { get; }

    public string CategoryName { get; }

    public string CategoryIcon { get; }

    public string CategoryColor { get; }

    public decimal Amount { get; }

    public string Type { get; }

    public DateTime Date { get; }

    public string Note { get; }

    public string TypeText => Type == TransactionTypes.Income ? "Thu nhập" : "Chi tiêu";

    public string AmountText
    {
        get
        {
            var sign = Type == TransactionTypes.Income ? "+" : "-";
            return $"{sign}{Amount:N0}đ";
        }
    }

    public string AmountColor => Type == TransactionTypes.Income ? "#419CDF" : "#F05A3A";
}
