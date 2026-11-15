namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;

public sealed record BoardPermissionModel
{
    public string Name { get; init; } = "";
    public string Label { get; init; } = "";
    public string? Description { get; init; }
    public string? RequiresName { get; init; }
    public string? RequiresLabel { get; init; }
}
