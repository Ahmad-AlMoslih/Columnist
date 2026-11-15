using Columnist.Contracts.Dtos;
using Columnist.Utilities.Constants;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed record GetBoardModel
{
    public IReadOnlyList<TokenOptionDto> Boards { get; init; } = [];
    public string? SelectedBoardToken { get; init; }
    public string? Search { get; init; }
    public string? SelectedBoardName { get; init; }
    public IReadOnlyList<GetBoardColumnModel> Columns { get; init; } = [];
    public BoardFilterModel Filter { get; init; } = new();

    // Shown tasks (after the search and the filter) and all of the board's tasks.
    public int TaskCount => Columns.Sum(c => c.Tasks.Count);
    public int TotalTaskCount => Columns.Sum(c => c.TotalTaskCount);

    public bool IsSearchActive => Search?.Trim() is { Length: >= SearchRules.MinLength };
    public bool IsFiltered => IsSearchActive || Filter.ActiveGroupCount > 0;

    // The search and filter query string (live refresh keeps them).
    public IEnumerable<KeyValuePair<string, string?>> ToQuery()
    {
        if (IsSearchActive)
            yield return new("search", Search!.Trim());
        foreach (var pair in Filter.ToQuery())
            yield return pair;
    }
}
