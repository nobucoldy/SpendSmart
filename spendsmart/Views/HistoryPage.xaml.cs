using System.Windows.Controls;
using spendsmart.Services;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class HistoryPage : UserControl
{
    public HistoryPage()
    {
        InitializeComponent();
        DataContext = new HistoryViewModel(ServiceFactory.CreateTransactionService());
        IsVisibleChanged += HistoryPage_IsVisibleChanged;
    }

    private void HistoryPage_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible && DataContext is HistoryViewModel viewModel)
        {
            viewModel.Refresh();
        }
    }

    private void MonthCalendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        MonthToggle.IsChecked = false;
    }
}
