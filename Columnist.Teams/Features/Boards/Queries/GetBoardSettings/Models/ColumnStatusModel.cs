namespace Columnist.Teams.Features.Boards.Queries.GetBoardSettings.Models;

public sealed record ColumnStatusModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Color { get; init; } = "";
}
