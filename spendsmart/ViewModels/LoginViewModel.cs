using System;
using System.Windows;
using System.Windows.Input;
using spendsmart.Services;
using spendsmart.Views;

namespace spendsmart.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AuthService authService;
    private string email = string.Empty;
    private string password = string.Empty;
    private string errorMessage = string.Empty;

    public LoginViewModel(AuthService authService)
    {
        this.authService = authService;
        LoginCommand = new RelayCommand(Login);
        OpenRegisterCommand = new RelayCommand(OpenRegister);
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

    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    public ICommand LoginCommand { get; }

    public ICommand OpenRegisterCommand { get; }

    private void Login()
    {
        ErrorMessage = string.Empty;

        try
        {
            var result = authService.Login(Email, Password);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
            CloseActiveWindow();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
    }

    private void OpenRegister()
    {
        var registerWindow = new RegisterWindow();
        registerWindow.Show();
        CloseActiveWindow();
    }

    private static void CloseActiveWindow()
    {
        Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive)
            ?.Close();
    }
}
