namespace Columnist.Teams.Features.Teams.Queries.GetMyTeams.Models;

public sealed record MyTeamModel
{
    public string Token { get; init; } = "";
    public string Name { get; init; } = "";
    public bool IsOwner { get; init; }
    public DateTime? AccessExpiresAt { get; init; }
    public bool IsExpired { get; init; }
    public bool IsCurrent { get; init; }
}
