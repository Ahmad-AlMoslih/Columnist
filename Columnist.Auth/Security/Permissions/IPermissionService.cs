namespace Columnist.Auth.Security.Permissions;

public interface IPermissionService
{
    Task<IReadOnlySet<string>> GetPermissionsAsync(int userId, CancellationToken ct = default);
}
