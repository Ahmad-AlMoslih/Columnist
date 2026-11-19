namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed record BoardTabModel
{
    public string Token { get; init; } = "";
    public string Name { get; init; } = "";
    public bool Selected { get; init; }
}
