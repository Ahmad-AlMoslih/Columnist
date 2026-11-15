using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Permissions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Columnist.Auth.Security.CurrentUser;

public sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPermissionService _permissionService;

    public UserContext(IHttpContextAccessor httpContextAccessor, IPermissionService permissionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _permissionService = permissionService;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId => int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? FullName => User?.FindFirstValue("full_name");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public async Task<bool> HasPermissionAsync(Permission permission, CancellationToken ct = default)
    {
        if (!IsAuthenticated || UserId is null) return false;
        var permissions = await _permissionService.GetPermissionsAsync(UserId.Value, ct);
        return permissions.Contains(permission.ToString());
    }
}
