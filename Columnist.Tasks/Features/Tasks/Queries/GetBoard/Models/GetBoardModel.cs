using Columnist.Contracts.Dtos;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed record GetBoardModel
{
    public IReadOnlyList<TokenOptionDto> Boards { get; init; } = [];
    public string? SelectedBoardToken { get; init; }
    public string? Search { get; init; }
    public string? SelectedBoardName { get; init; }
    public IReadOnlyList<GetBoardColumnModel> Columns { get; init; } = [];

    public int TaskCount => Columns.Sum(c => c.Tasks.Count);
}
