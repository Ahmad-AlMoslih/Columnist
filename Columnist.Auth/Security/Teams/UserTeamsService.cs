using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Dapper;

namespace Columnist.Auth.Security.Teams;

public sealed class UserTeamsService : IUserTeamsService
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly DateTime _now = DateTime.Now;   // the DB stores local times (SYSDATETIME)
    private IReadOnlyList<TeamDto>? _teams;

    public UserTeamsService(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<IReadOnlyList<TeamDto>> GetAsync(CancellationToken ct = default)
    {
        if (_teams is not null)
            return _teams;

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", _userContext.UserId);

        _teams = await _db.QueryAsync<TeamDto>("dbo.GetUserTeams", parameters, ct);
        return _teams;
    }

    public async Task<TeamDto?> GetCurrentAsync(string? teamToken, CancellationToken ct = default)
    {
        var teams = await GetAsync(ct);
        var currentId = string.IsNullOrEmpty(teamToken) ? null : teamToken.Decrypt<int>();
        return teams.FirstOrDefault(t => t.Id == currentId && IsActive(t)) ?? teams.FirstOrDefault(IsActive);
    }

    public async Task<bool> IsOwnerAsync(int teamId, CancellationToken ct = default) =>
        (await GetAsync(ct)).Any(t => t.Id == teamId && t.IsOwner);

    public bool IsActive(TeamDto team) => team.IsOwner || team.AccessExpiresAt > _now;
}
