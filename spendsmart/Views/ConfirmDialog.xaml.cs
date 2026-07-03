using System.Windows;

namespace spendsmart.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog(string title, string message, string confirmText = "Xóa", string cancelText = "Hủy")
    {
        InitializeComponent();

        TitleText = title;
        MessageText = message;
        ConfirmText = confirmText;
        CancelText = cancelText;
        DataContext = this;
    }

    public string TitleText { get; }

    public string MessageText { get; }

    public string ConfirmText { get; }

    public string CancelText { get; }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
