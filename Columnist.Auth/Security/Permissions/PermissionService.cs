using Columnist.DataAccess;
using Dapper;

namespace Columnist.Auth.Security.Permissions;

public sealed class PermissionService : IPermissionService
{
    private readonly IDbExecutor _db;
    private readonly Dictionary<int, IReadOnlySet<string>> _loaded = new();

    public PermissionService(IDbExecutor db)
    {
        _db = db;
    }

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(int userId, CancellationToken ct = default)
    {
        if (_loaded.TryGetValue(userId, out var loaded))
            return loaded;

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        var name = await _db.QueryAsync<string>("dbo.GetAnyBoardPermissions", parameters, ct);

        IReadOnlySet<string> set = new HashSet<string>(name, StringComparer.OrdinalIgnoreCase);
        _loaded[userId] = set;
        return set;
    }
}
