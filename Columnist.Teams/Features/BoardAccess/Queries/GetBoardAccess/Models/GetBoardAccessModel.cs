namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;

public sealed record GetBoardAccessModel
{
    public string BoardToken { get; init; } = "";
    public string? BoardName { get; init; }
    public string? Search { get; init; }
    public IReadOnlyList<BoardMemberModel> Members { get; init; } = [];
    public IReadOnlyList<BoardCandidateModel> Candidates { get; init; } = [];
    public IReadOnlyList<BoardPermissionModel> Permissions { get; init; } = [];

    // The Super Admin's row (the signed-in user: only the owner can open this page).
    public string OwnerName { get; init; } = "";
    public string OwnerInitials { get; init; } = "";
    public string? OwnerPhotoUrl { get; init; }

    // The board menu's links.
    public bool CanSeeDeleted { get; init; }
    public bool CanSeeLogs { get; init; }
    public bool CanManage { get; init; }
}
