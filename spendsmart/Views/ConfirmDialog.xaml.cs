using System.Windows;

namespace spendsmart.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog(
        string title,
        string message,
        string confirmText = "Xóa",
        string cancelText = "Hủy",
        string iconText = "!",
        string iconBackground = "#FFF1EE",
        string iconForeground = "#F05A3A",
        string confirmBackground = "#F05A3A")
    {
        InitializeComponent();

        TitleText = title;
        MessageText = message;
        ConfirmText = confirmText;
        CancelText = cancelText;
        IconText = iconText;
        IconBackground = iconBackground;
        IconForeground = iconForeground;
        ConfirmBackground = confirmBackground;
        DataContext = this;
    }

    public string TitleText { get; }

    public string MessageText { get; }

    public string ConfirmText { get; }

    public string CancelText { get; }

    public string IconText { get; }

    public string IconBackground { get; }

    public string IconForeground { get; }

    public string ConfirmBackground { get; }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
