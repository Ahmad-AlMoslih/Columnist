namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;

public sealed record GetBoardAccessModel
{
    public string BoardToken { get; init; } = "";
    public string? BoardName { get; init; }
    public string? Search { get; init; }
    public IReadOnlyList<BoardMemberModel> Members { get; init; } = [];
    public IReadOnlyList<BoardCandidateModel> Candidates { get; init; } = [];
    public IReadOnlyList<BoardPermissionModel> Permissions { get; init; } = [];
}
