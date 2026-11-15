namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;

public sealed record BoardPermissionChipModel
{
    public string PermissionName { get; init; } = "";
    public string Label { get; init; } = "";
}
