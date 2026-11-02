namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed record GetBoardTaskModel
{
    public string Token { get; init; } = "";
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string ColumnName { get; init; } = "";
    public string? AssigneeName { get; init; }
    public DateTime? DueDate { get; init; }
    public int ImageCount { get; init; }
    public string? FirstImageUrl { get; init; }
}
