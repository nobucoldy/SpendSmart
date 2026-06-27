using System.Windows.Controls;
using System.Windows.Input;
using spendsmart.Views;

namespace spendsmart.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly InputPage inputPage = new();
    private readonly HistoryPage historyPage = new();
    private readonly ReportPage reportPage = new();
    private readonly CategoryManagementPage categoryManagementPage = new();
    private readonly MorePage morePage = new();
    private UserControl currentPage;
    private string selectedTab = "Input";

    public MainViewModel()
    {
        currentPage = inputPage;
        NavigateCommand = new RelayCommand(Navigate);
    }

    public UserControl CurrentPage
    {
        get => currentPage;
        private set => SetProperty(ref currentPage, value);
    }

    public string SelectedTab
    {
        get => selectedTab;
        private set
        {
            if (SetProperty(ref selectedTab, value))
            {
                OnPropertyChanged(nameof(IsInputSelected));
                OnPropertyChanged(nameof(IsHistorySelected));
                OnPropertyChanged(nameof(IsReportsSelected));
                OnPropertyChanged(nameof(IsCategoriesSelected));
                OnPropertyChanged(nameof(IsMoreSelected));
            }
        }
    }

    public bool IsInputSelected => SelectedTab == "Input";

    public bool IsHistorySelected => SelectedTab == "History";

    public bool IsReportsSelected => SelectedTab == "Reports";

    public bool IsCategoriesSelected => SelectedTab == "Categories";

    public bool IsMoreSelected => SelectedTab == "More";

    public ICommand NavigateCommand { get; }

    private void Navigate(object? tab)
    {
        if (tab is not string tabName)
        {
            return;
        }

        CurrentPage = tabName switch
        {
            "Input" => inputPage,
            "History" => historyPage,
            "Reports" => reportPage,
            "Categories" => categoryManagementPage,
            "More" => morePage,
            _ => CurrentPage
        };

        SelectedTab = tabName;
    }
}
