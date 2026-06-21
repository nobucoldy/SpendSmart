using spendsmart.Models;

namespace spendsmart.Services;

public class ApplicationState
{
    public User? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser is not null;

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
    }

    public void ClearCurrentUser()
    {
        CurrentUser = null;
    }
}
