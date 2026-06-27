using System.Windows.Controls;
using spendsmart.Services;

namespace spendsmart.Views;

public partial class MorePage : UserControl
{
    public MorePage()
    {
        InitializeComponent();
        DataContext = ServiceFactory.CreateMoreViewModel();
    }
}
