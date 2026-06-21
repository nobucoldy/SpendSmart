using spendsmart.Models;

namespace spendsmart.Services;

public class AuthResult
{
    private AuthResult(bool success, string message, User? user)
    {
        Success = success;
        Message = message;
        User = user;
    }

    public bool Success { get; }

    public string Message { get; }

    public User? User { get; }

    public static AuthResult Ok(User user, string message)
    {
        return new AuthResult(true, message, user);
    }

    public static AuthResult Fail(string message)
    {
        return new AuthResult(false, message, null);
    }
}
