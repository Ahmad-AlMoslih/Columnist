using Columnist.Auth.Authorization;

namespace Columnist.Auth.Security.CurrentUser;

public interface IUserContext
{
    int? UserId { get; }
    string? FullName { get; }
    bool IsAuthenticated { get; }
    Task<bool> HasPermissionAsync(Permission permission, CancellationToken ct = default);
}
