using System.Windows;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
