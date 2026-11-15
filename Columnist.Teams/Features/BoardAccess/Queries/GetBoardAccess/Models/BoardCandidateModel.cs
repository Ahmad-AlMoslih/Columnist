namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;

public sealed record BoardCandidateModel
{
    public string UserToken { get; init; } = "";
    public string FullName { get; init; } = "";
    public string Initials { get; init; } = "";
    public string? PhotoUrl { get; init; }   // profile photo; null = show the initials
    public bool IsExpired { get; init; }
}
