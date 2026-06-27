using System.Windows.Controls;
using spendsmart.Services;
using spendsmart.ViewModels;

namespace spendsmart.Views;

public partial class ReportPage : UserControl
{
    public ReportPage()
    {
        InitializeComponent();
        DataContext = new ReportViewModel(ServiceFactory.CreateReportService());
        IsVisibleChanged += ReportPage_IsVisibleChanged;
    }

    private void ReportPage_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible && DataContext is ReportViewModel viewModel)
        {
            viewModel.Refresh();
        }
    }
}
