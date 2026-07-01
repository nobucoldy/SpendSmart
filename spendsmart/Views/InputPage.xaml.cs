using System.Windows.Controls;
using spendsmart.Services;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class InputPage : UserControl
{
    public InputPage()
    {
        InitializeComponent();
        DataContext = new InputViewModel(
            ServiceFactory.CreateCategoryService(),
            ServiceFactory.CreateTransactionService());
    }

    private void DateCalendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        DateToggle.IsChecked = false;
    }
}
