using Columnist.Contracts.Dtos;

namespace Columnist.Auth.Security.Teams;

// The signed-in user's teams (dbo.GetUserTeams), read at most once per request.
public interface IUserTeamsService
{
    Task<IReadOnlyList<TeamDto>> GetAsync(CancellationToken ct = default);

    // The team the user works in: the given team if still active, otherwise the first active team, otherwise null.
    Task<TeamDto?> GetCurrentAsync(string? teamToken, CancellationToken ct = default);

    Task<bool> IsOwnerAsync(int teamId, CancellationToken ct = default);

    // Owner, or a member whose access hasn't expired.
    bool IsActive(TeamDto team);
}
