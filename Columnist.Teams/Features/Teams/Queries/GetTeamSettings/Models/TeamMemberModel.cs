namespace Columnist.Teams.Features.Teams.Queries.GetTeamSettings.Models;

public sealed record TeamMemberModel
{
    public string UserToken { get; init; } = "";
    public string FullName { get; init; } = "";
    public string? Username { get; init; }
    public string Initials { get; init; } = "";
    public string? PhotoUrl { get; init; }   // profile photo; null = show the initials
    public bool IsOwner { get; init; }
    public DateTime JoinedAt { get; init; }
    public DateTime? AccessExpiresAt { get; init; }
    public bool IsExpired { get; init; }
    public int? DaysLeft { get; init; }
    public int BoardCount { get; init; }
    public int TaskCount { get; init; }
}
