using System.Globalization;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

// The board filter's current values (only the valid ones) and the people it offers.
// A selected person's Token is the token the request sent, so the open filter panel and the chips name them the same way.
public sealed record BoardFilterModel
{
    public const string DueOverdue = "overdue";
    public const string DueToday = "today";
    public const string DueWeek = "week";
    public const string DueNone = "none";
    public const string DueCustom = "custom";

    // Query value → status lookup name.
    public static readonly IReadOnlyList<KeyValuePair<string, string>> StatusOptions =
    [
        new("ToDo", "To Do"),
        new("InProgress", "In Progress"),
        new("Done", "Done")
    ];

    public static readonly IReadOnlyList<KeyValuePair<string, string>> DueOptions =
    [
        new("", "Any"),
        new(DueOverdue, "Overdue"),
        new(DueToday, "Today"),
        new(DueWeek, "This week"),
        new(DueNone, "No due date"),
        new(DueCustom, "Custom range")
    ];

    public string MeToken { get; init; } = "";
    public bool MeSelected { get; init; }
    public IReadOnlyList<BoardFilterMemberModel> Members { get; init; } = [];   // without the current user
    public bool Unassigned { get; init; }
    public string? Due { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public IReadOnlyList<string> Statuses { get; init; } = [];   // query values, in StatusOptions order
    public bool Unfinished { get; init; }

    public bool HasAssignee => MeSelected || Unassigned || Members.Any(m => m.Selected);

    // Assignee, Due, Status and Checklist; the search isn't counted.
    public int ActiveGroupCount =>
        (HasAssignee ? 1 : 0) + (Due is not null ? 1 : 0) + (Statuses.Count > 0 ? 1 : 0) + (Unfinished ? 1 : 0);

    // The query string for these values (names as in BoardFilterRequest).
    public IEnumerable<KeyValuePair<string, string?>> ToQuery()
    {
        if (MeSelected)
            yield return new("assignees", MeToken);
        foreach (var member in Members.Where(m => m.Selected))
            yield return new("assignees", member.Token);
        if (Unassigned)
            yield return new("unassigned", "1");
        if (Due is not null)
            yield return new("due", Due);
        if (From is not null)
            yield return new("from", From.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (To is not null)
            yield return new("to", To.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        foreach (var status in Statuses)
            yield return new("status", status);
        if (Unfinished)
            yield return new("unfinished", "1");
    }
}
