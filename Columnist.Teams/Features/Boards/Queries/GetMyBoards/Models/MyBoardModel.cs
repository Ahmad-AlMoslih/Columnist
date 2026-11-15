namespace Columnist.Teams.Features.Boards.Queries.GetMyBoards.Models;

public sealed record MyBoardModel
{
    public string Token { get; init; } = "";
    public string Name { get; init; } = "";
    public bool IsCurrent { get; init; }
}
