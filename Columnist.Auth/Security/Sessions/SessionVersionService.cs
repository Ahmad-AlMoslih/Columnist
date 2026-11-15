using Columnist.DataAccess;
using Dapper;

namespace Columnist.Auth.Security.Sessions;

public sealed class SessionVersionService : ISessionVersionService
{
    private readonly IDbExecutor _db;

    public SessionVersionService(IDbExecutor db)
    {
        _db = db;
    }

    public async Task<bool> IsCurrentAsync(int userId, int sessionVersion, CancellationToken ct = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        var current = await _db.QuerySingleOrDefaultAsync<int?>("dbo.GetUserSessionVersion", parameters, ct);
        return current == sessionVersion;
    }
}
