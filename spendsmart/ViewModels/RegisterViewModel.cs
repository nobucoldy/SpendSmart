using System;
using System.Windows;
using System.Windows.Input;
using spendsmart.Services;
using spendsmart.Views;

namespace spendsmart.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly AuthService authService;
    private string fullName = string.Empty;
    private string email = string.Empty;
    private string password = string.Empty;
    private string confirmPassword = string.Empty;
    private string errorMessage = string.Empty;

    public RegisterViewModel(AuthService authService)
    {
        this.authService = authService;
        RegisterCommand = new RelayCommand(Register);
        OpenLoginCommand = new RelayCommand(OpenLogin);
    }

    public string FullName
    {
        get => fullName;
        set => SetProperty(ref fullName, value);
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    public string ConfirmPassword
    {
        get => confirmPassword;
        set => SetProperty(ref confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    public ICommand RegisterCommand { get; }

    public ICommand OpenLoginCommand { get; }

    private void Register()
    {
        ErrorMessage = string.Empty;

        try
        {
            var result = authService.Register(FullName, Email, Password, ConfirmPassword);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            var currentWindow = GetActiveWindow();
            var mainWindow = new MainWindow();
            mainWindow.Show();
            currentWindow?.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Đăng ký thất bại: {ex.Message}";
        }
    }

    private void OpenLogin()
    {
        var currentWindow = GetActiveWindow();
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        currentWindow?.Close();
    }

    private static Window? GetActiveWindow()
    {
        return Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive);
    }
}
