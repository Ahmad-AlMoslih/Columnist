namespace Columnist.Auth.Security.CurrentUser;

public interface IUserContext
{
    int? UserId { get; }
    string? FullName { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
