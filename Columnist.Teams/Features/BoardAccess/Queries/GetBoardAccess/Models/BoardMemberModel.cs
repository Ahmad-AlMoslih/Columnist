namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;

public sealed record BoardMemberModel
{
    public string UserToken { get; init; } = "";
    public string FullName { get; init; } = "";
    public string? Username { get; init; }
    public string Initials { get; init; } = "";
    public string? PhotoUrl { get; init; }   // profile photo; null = show the initials
    public bool IsExpired { get; init; }
    public int DaysLeft { get; init; }
    public int AssignedTaskCount { get; init; }
    public IReadOnlyList<BoardPermissionChipModel> Chips { get; init; } = [];
}
