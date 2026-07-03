using System.Windows;
using System.Windows.Input;
using spendsmart.Services;
using spendsmart.Views;

namespace spendsmart.ViewModels;

public class MoreViewModel : BaseViewModel
{
    private readonly AuthService authService;
    private readonly ApplicationState applicationState;

    public MoreViewModel(AuthService authService, ApplicationState applicationState)
    {
        this.authService = authService;
        this.applicationState = applicationState;
        LogoutCommand = new RelayCommand(Logout);
        ShowAboutCommand = new RelayCommand(ShowAbout);
    }

    public string FullName => applicationState.CurrentUser?.FullName ?? "Guest";

    public string Email => applicationState.CurrentUser?.Email ?? "Not logged in";

    public ICommand LogoutCommand { get; }

    public ICommand ShowAboutCommand { get; }

    private void Logout()
    {
        var activeWindow = Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive);

        var dialog = new ConfirmDialog(
            "Đăng xuất",
            "Bạn có chắc chắn muốn đăng xuất khỏi SpendSmart không?",
            "Đăng xuất",
            "Hủy",
            "↩",
            "#EEF8FE",
            "#159CE4",
            "#159CE4");

        if (activeWindow is not null)
        {
            dialog.Owner = activeWindow;
        }

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        authService.Logout();

        var loginWindow = new LoginWindow();
        loginWindow.Show();

        Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window is MainWindow)
            ?.Close();
    }

    private static void ShowAbout()
    {
        var aboutWindow = new AboutWindow
        {
            Owner = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(window => window.IsActive)
        };

        aboutWindow.ShowDialog();
    }
}
