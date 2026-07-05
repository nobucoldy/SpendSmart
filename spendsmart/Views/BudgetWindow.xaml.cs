using System.Windows;
using spendsmart.Services;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class BudgetWindow : Window
{
    public BudgetWindow()
    {
        InitializeComponent();
        DataContext = new BudgetViewModel(ServiceFactory.CreateBudgetService(), ServiceFactory.CreateCategoryService());
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
