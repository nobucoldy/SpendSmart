using System.Windows;
using spendsmart.Services;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (!ServiceFactory.ApplicationState.IsLoggedIn)
        {
            Loaded += (_, _) =>
            {
                new LoginWindow().Show();
                Close();
            };
            return;
        }

        DataContext = new MainViewModel();
    }
}
