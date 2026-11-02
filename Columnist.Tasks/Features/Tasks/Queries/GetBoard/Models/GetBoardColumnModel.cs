namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed record GetBoardColumnModel
{
    public string ColumnToken { get; init; } = "";
    public string Name { get; init; } = "";
    public string StatusColor { get; init; } = "";
    public IReadOnlyList<GetBoardTaskModel> Tasks { get; init; } = [];
}
