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
        ShowBudgetCommand = new RelayCommand(ShowBudget);
    }

    public string FullName => applicationState.CurrentUser?.FullName ?? "Khách";

    public string Email => applicationState.CurrentUser?.Email ?? "Chưa đăng nhập";

    public ICommand LogoutCommand { get; }

    public ICommand ShowAboutCommand { get; }

    public ICommand ShowBudgetCommand { get; }

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

    private static void ShowBudget()
    {
        var budgetWindow = new BudgetWindow
        {
            Owner = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(window => window.IsActive)
        };

        budgetWindow.ShowDialog();
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
