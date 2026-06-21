using System.Windows;
using System.Windows.Controls;
using spendsmart.Services;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
        DataContext = new RegisterViewModel(ServiceFactory.CreateAuthService());
    }

    private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
    }

    private void ConfirmPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.ConfirmPassword = passwordBox.Password;
        }
    }
}
