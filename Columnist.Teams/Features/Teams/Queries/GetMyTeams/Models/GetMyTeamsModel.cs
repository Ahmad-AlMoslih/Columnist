namespace Columnist.Teams.Features.Teams.Queries.GetMyTeams.Models;

public sealed record GetMyTeamsModel
{
    public IReadOnlyList<MyTeamModel> Teams { get; init; } = [];
    public string? CurrentTeamToken { get; init; }
}
