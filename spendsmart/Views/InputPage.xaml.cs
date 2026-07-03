using System.Windows.Controls;
using spendsmart.Services;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class InputPage : UserControl
{
    public InputPage()
        : this(null)
    {
    }

    public InputPage(Action? editCompleted)
    {
        InitializeComponent();
        DataContext = new InputViewModel(
            ServiceFactory.CreateCategoryService(),
            ServiceFactory.CreateTransactionService(),
            editCompleted,
            ConfirmDelete);
    }

    private void DateCalendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        DateToggle.IsChecked = false;
    }

    private bool ConfirmDelete()
    {
        var dialog = new ConfirmDialog(
            "Xác nhận xóa",
            "Bạn có chắc chắn muốn xóa khoản thu chi này không?");

        var owner = System.Windows.Window.GetWindow(this);
        if (owner is not null)
        {
            dialog.Owner = owner;
        }

        return dialog.ShowDialog() == true;
    }
}
