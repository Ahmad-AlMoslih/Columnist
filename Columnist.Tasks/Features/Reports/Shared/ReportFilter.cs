using System.Globalization;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Constants;

namespace Columnist.Tasks.Features.Reports.Shared;

// The report filter as read from ReportFilterRequest (invalid values dropped), shared by the report page and the CSV.
public sealed record ReportFilter
{
    public string? Search { get; init; }                  // as typed (trimmed)
    public string? AssigneeToken { get; init; }           // the token it was sent with
    public int? AssigneeId { get; init; }                 // a board person, else null
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public IReadOnlyDictionary<int, string> ColumnTokens { get; init; } = new Dictionary<int, string>();   // chosen column id → the token it was sent with

    public static ReportFilter Read(ReportFilterRequest? request, BoardReportData data)
    {
        request ??= new ReportFilterRequest();

        var assigneeId = string.IsNullOrEmpty(request.Assignee) ? null : request.Assignee.Decrypt<int>();
        if (assigneeId is not null && data.People.All(p => p.Id != assigneeId))
            assigneeId = null;

        var from = ParseDate(request.From);
        var to = ParseDate(request.To);
        if (from > to)
            from = to = null;

        var columnTokens = new Dictionary<int, string>();
        foreach (var token in request.Column ?? [])
        {
            var id = token.Decrypt<int>();
            if (id is not null && data.Columns.Any(c => c.ColumnId == id))
                columnTokens.TryAdd(id.Value, token);
        }

        return new ReportFilter
        {
            Search = request.Q?.Trim() is { Length: > 0 } search ? search : null,
            AssigneeToken = assigneeId is null ? null : request.Assignee,
            AssigneeId = assigneeId,
            From = from,
            To = to,
            ColumnTokens = columnTokens
        };
    }

    // Search (2+ characters), assignee, due range (tasks without a due date drop out) and columns, ANDed.
    public bool Matches(ReportTask task)
    {
        if (Search is { Length: >= SearchRules.MinLength } && !task.Title.Contains(Search, StringComparison.OrdinalIgnoreCase))
            return false;
        if (AssigneeId is { } assignee && !task.AssigneeIds.Contains(assignee))
            return false;
        if ((From is not null || To is not null) && (task.DueDate is null || task.DueDate < From || task.DueDate > To))
            return false;
        if (ColumnTokens.Count > 0 && !ColumnTokens.ContainsKey(task.ColumnId))
            return false;
        return true;
    }

    // The query string for these values (names as in ReportFilterRequest).
    public IEnumerable<KeyValuePair<string, string?>> ToQuery()
    {
        if (Search is not null)
            yield return new("q", Search);
        if (AssigneeToken is not null)
            yield return new("assignee", AssigneeToken);
        if (From is not null)
            yield return new("from", From.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (To is not null)
            yield return new("to", To.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        foreach (var token in ColumnTokens.Values)
            yield return new("column", token);
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
}
