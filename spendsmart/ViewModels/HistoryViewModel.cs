using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using spendsmart.Constants;
using spendsmart.Models;
using spendsmart.Services;

namespace spendsmart.ViewModels;

public class HistoryViewModel : BaseViewModel
{
    private readonly TransactionService transactionService;
    private DateTime selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private decimal totalIncome;
    private decimal totalExpense;
    private decimal balance;

    public HistoryViewModel(TransactionService transactionService)
    {
        this.transactionService = transactionService;
        Transactions = new ObservableCollection<TransactionHistoryItemViewModel>();
        PreviousMonthCommand = new RelayCommand(() => ChangeMonth(-1));
        NextMonthCommand = new RelayCommand(() => ChangeMonth(1));
        RefreshCommand = new RelayCommand(Refresh);

        Refresh();
    }

    public ObservableCollection<TransactionHistoryItemViewModel> Transactions { get; }

    public string MonthText
    {
        get
        {
            var lastDay = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);
            return $"{selectedMonth:MM/yyyy}  (01/{selectedMonth:MM}–{lastDay:00}/{selectedMonth:MM})";
        }
    }

    public string TotalIncomeText => FormatMoney(totalIncome);

    public string TotalExpenseText => FormatMoney(totalExpense);

    public string BalanceText => FormatMoney(balance, includeSign: true);

    public string EmptyMessage => Transactions.Count == 0 ? "Chưa có giao dịch trong tháng này." : string.Empty;

    public ICommand PreviousMonthCommand { get; }

    public ICommand NextMonthCommand { get; }

    public ICommand RefreshCommand { get; }

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
        foreach (var transaction in transactions)
        {
            Transactions.Add(new TransactionHistoryItemViewModel(transaction));
        }

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

    private static string FormatMoney(decimal amount, bool includeSign = false)
    {
        var prefix = includeSign && amount > 0 ? "+" : string.Empty;
        return $"{prefix}{amount:N0}đ";
    }
}

public sealed class TransactionHistoryItemViewModel
{
    public TransactionHistoryItemViewModel(Transaction transaction)
    {
        CategoryName = transaction.Category?.Name ?? "Unknown";
        CategoryIcon = CategoryItemViewModel.GetIconSymbol(transaction.Category?.IconName ?? string.Empty);
        CategoryColor = transaction.Category?.Color ?? "#666666";
        Amount = transaction.Amount;
        Type = transaction.Type;
        Date = transaction.Date;
        Note = transaction.Note ?? string.Empty;
    }

    public string CategoryName { get; }

    public string CategoryIcon { get; }

    public string CategoryColor { get; }

    public decimal Amount { get; }

    public string Type { get; }

    public DateTime Date { get; }

    public string Note { get; }

    public string DateText => Date.ToString("dd/MM/yyyy (ddd)", CultureInfo.CurrentCulture);

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
