namespace spendsmart.Services;

using spendsmart.ViewModels;

public static class ServiceFactory
{
    public static ApplicationState ApplicationState { get; } = new();

    public static AuthService CreateAuthService()
    {
        return new AuthService(ApplicationState);
    }

    public static CategoryService CreateCategoryService()
    {
        return new CategoryService(ApplicationState);
    }

    public static TransactionService CreateTransactionService()
    {
        return new TransactionService(ApplicationState);
    }

    public static ReportService CreateReportService()
    {
        return new ReportService(CreateTransactionService());
    }

        public static BudgetService CreateBudgetService()
    {
        return new BudgetService(ApplicationState);
    }

    public static MoreViewModel CreateMoreViewModel()
    {
        return new MoreViewModel(CreateAuthService(), ApplicationState);
    }
}
