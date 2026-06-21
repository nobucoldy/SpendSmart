namespace spendsmart.Services;

public static class ServiceFactory
{
    public static ApplicationState ApplicationState { get; } = new();

    public static AuthService CreateAuthService()
    {
        return new AuthService(ApplicationState);
    }
}
