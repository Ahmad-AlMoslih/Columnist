using Columnist.Tasks.Features.Tasks.Models;

namespace Columnist.Tasks.Features.Reports.Shared.Models;

// A task row: the title, the chips (column, due date, checklist, an extra one), an optional detail line and the assignees.
// Clicking it opens the task popup.
public sealed record ReportTaskRowModel
{
    public string Token { get; init; } = "";
    public string Title { get; init; } = "";
    public string ColumnName { get; init; } = "";
    public string? ColumnColor { get; init; }   // the column's status colour (#RRGGBB), null = neutral
    public string? DueText { get; init; }       // "Overdue · Nov 12", "Today · Nov 18", "Nov 25"
    public ReportTone? DueTone { get; init; }   // red, amber, or null = grey
    public int ChecklistDone { get; init; }
    public int ChecklistTotal { get; init; }
    public string? ExtraChip { get; init; }     // "On time", "3 days late"
    public ReportTone? ExtraTone { get; init; }
    public int? Progress { get; init; }         // checklist progress bar, 0-100
    public string? Detail { get; init; }
    public IReadOnlyList<TaskAssigneeModel> Assignees { get; init; } = [];
}
