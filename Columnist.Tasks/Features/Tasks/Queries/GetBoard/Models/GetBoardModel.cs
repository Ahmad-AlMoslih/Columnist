using Columnist.Utilities.Constants;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed record GetBoardModel
{
    public IReadOnlyList<BoardTabModel> Boards { get; init; } = [];
    public string? SelectedBoardToken { get; init; }
    public string? Search { get; init; }
    public string? SelectedBoardName { get; init; }
    public IReadOnlyList<GetBoardColumnModel> Columns { get; init; } = [];
    public BoardFilterModel Filter { get; init; } = new();

    // What the user may do on the selected board.
    public bool CanWrite { get; init; }
    public bool CanSeeDeleted { get; init; }
    public bool CanSeeLogs { get; init; }
    public bool CanSeeReports { get; init; }
    public bool CanManage { get; init; }   // Super Admin of the board's team

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
