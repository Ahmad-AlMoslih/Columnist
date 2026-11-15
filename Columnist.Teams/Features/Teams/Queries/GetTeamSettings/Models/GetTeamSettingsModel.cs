namespace Columnist.Teams.Features.Teams.Queries.GetTeamSettings.Models;

public sealed record GetTeamSettingsModel
{
    public string Token { get; init; } = "";
    public string Name { get; init; } = "";
    public string InviteCode { get; init; } = "";
    public DateTime InviteCodeExpiresAt { get; init; }
    public IReadOnlyList<TeamMemberModel> Members { get; init; } = [];
    public int BoardCount { get; init; }
}
