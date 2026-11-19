using Columnist.Contracts.Dtos;

namespace Columnist.Tasks.Features.Reports.Shared;

// How audit entries are counted per person (report "Activity per person", CSV) and written as sentences (dashboard "Recent activity").
public static class ActivityRules
{
    public const int Days = 7;

    public enum Kind { Move, Edit, Comment, Tick, Created }

    // Board and Access entries don't count (null).
    public static Kind? KindOf(string action) => action switch
    {
        "TaskMoved" => Kind.Move,
        "TaskEdited" or "TaskAssigned" or "TaskDeleted" or "TaskRestored"
            or "ChecklistAdded" or "ChecklistRenamed" or "ChecklistDeleted"
            or "ChecklistItemAdded" or "ChecklistItemEdited" or "ChecklistItemDeleted" => Kind.Edit,
        "CommentAdded" or "CommentEdited" => Kind.Comment,
        "ChecklistItemChecked" or "ChecklistItemUnchecked" => Kind.Tick,
        "TaskCreated" => Kind.Created,
        _ => null
    };

    // One person's counts over the period.
    public sealed record PersonActivity(int UserId, int Moves, int Edits, int Comments, int Ticks, int Created)
    {
        public int Changes => Moves + Edits + Comments + Ticks + Created;

        // "5 moves · 4 edits · 3 comments" (only what they did).
        public string Breakdown => string.Join(" · ", new[]
        {
            Moves > 0 ? ReportRows.Count(Moves, "move", "moves") : null,
            Edits > 0 ? ReportRows.Count(Edits, "edit", "edits") : null,
            Comments > 0 ? ReportRows.Count(Comments, "comment", "comments") : null,
            Ticks > 0 ? ReportRows.Count(Ticks, "checklist tick", "checklist ticks") : null,
            Created > 0 ? ReportRows.Count(Created, "task created", "tasks created") : null
        }.Where(part => part is not null));
    }

    public static IReadOnlyDictionary<int, PersonActivity> PerPerson(IEnumerable<ActivityCountDto> rows) =>
        rows
            .Select(r => (r.UserId, Kind: KindOf(r.Action), r.Total))
            .Where(r => r.Kind is not null)
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => new PersonActivity(
                g.Key,
                g.Where(r => r.Kind == Kind.Move).Sum(r => r.Total),
                g.Where(r => r.Kind == Kind.Edit).Sum(r => r.Total),
                g.Where(r => r.Kind == Kind.Comment).Sum(r => r.Total),
                g.Where(r => r.Kind == Kind.Tick).Sum(r => r.Total),
                g.Where(r => r.Kind == Kind.Created).Sum(r => r.Total)));

    // The sentence after the actor's name, worded like the Logs page: "moved Design hero section to Review".
    // permissionLabel is the permission's label ("Logs · Read") for permission entries.
    public static string Text(ActivityEntryDto entry, string? permissionLabel)
    {
        var target = entry.TargetName;
        return entry.Action switch
        {
            "TaskCreated" => entry.NewValue is null ? $"created {target}" : $"created {target} in {entry.NewValue}",
            "TaskEdited" when entry.Field == "Images" => $"{(entry.NewValue == "Removed" ? "removed" : entry.NewValue?.ToLowerInvariant())} images on {target}",
            "TaskEdited" when entry.Field is not null => $"edited the {entry.Field.ToLowerInvariant()} of {target}",
            "TaskEdited" => $"edited {target}",
            "TaskMoved" => entry.NewValue is null ? $"moved {target}" : $"moved {target} to {entry.NewValue}",
            "TaskAssigned" => $"changed the assignees of {target}",
            "TaskDeleted" => $"deleted {target}",
            "TaskRestored" => $"restored {target}",
            "CommentAdded" => $"commented on {target}",
            "CommentEdited" => $"edited a comment on {target}",
            "ChecklistAdded" => $"added the checklist {entry.NewValue} to {target}",
            "ChecklistRenamed" => $"renamed a checklist on {target}",
            "ChecklistDeleted" => $"deleted the checklist {entry.OldValue} from {target}",
            "ChecklistItemAdded" => $"added {entry.NewValue} to {entry.Field} on {target}",
            "ChecklistItemEdited" => $"edited an item in {entry.Field} on {target}",
            "ChecklistItemDeleted" => $"deleted {entry.OldValue} from {entry.Field} on {target}",
            "ChecklistItemChecked" => $"checked {entry.NewValue} in {entry.Field} on {target}",
            "ChecklistItemUnchecked" => $"unchecked {entry.NewValue} in {entry.Field} on {target}",
            "BoardCreated" => $"created the board {target}",
            "BoardRenamed" => "renamed the board",
            "ColumnAdded" => $"added the column {entry.NewValue}",
            "ColumnRenamed" => "renamed a column",
            "ColumnDeleted" => $"deleted the column {entry.OldValue}",
            "BoardDeleted" => $"deleted the board {target}",
            "MemberAdded" => $"added {target} to this board",
            "MemberRemoved" => $"removed {target} from this board",
            "PermissionGranted" => $"gave {permissionLabel ?? entry.NewValue} to {target}",
            "PermissionRemoved" => $"removed {permissionLabel ?? entry.NewValue} from {target}",
            _ => $"{entry.Action} {target}"
        };
    }
}
