using System.Windows.Controls;
using spendsmart.Services;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class CategoryManagementPage : UserControl
{
    public CategoryManagementPage()
    {
        InitializeComponent();
        DataContext = new CategoryManagementViewModel(ServiceFactory.CreateCategoryService());
    }
}
