namespace Columnist.Tasks.Features.Reports.Shared.Models;

// One card of the dashboard or the report: the header (icon tile, label, note, number) and its rows.
// A card has one kind of row: tasks, activity entries (dashboard) or people (report's Activity per person).
public sealed record ReportCardModel
{
    public string Key { get; init; } = "";      // picks the icon; also the card's data-* hook
    public string Label { get; init; } = "";
    public string Note { get; init; } = "";
    public int Count { get; init; }
    public ReportTone Tone { get; init; } = ReportTone.Grey;
    public ReportPersonModel? Person { get; init; }   // a person's card shows their avatar in the tile

    public IReadOnlyList<ReportTaskRowModel> Tasks { get; init; } = [];
    public IReadOnlyList<ReportActivityModel> Activity { get; init; } = [];
    public IReadOnlyList<ReportPersonRowModel> People { get; init; } = [];

    public int RowCount => Tasks.Count + Activity.Count + People.Count;
}

// The card colours: foreground (icon, number) and tint (tile).
public sealed record ReportTone(string Color, string Tint)
{
    public static readonly ReportTone Red = new("#9B2C1A", "#FBE4DF");
    public static readonly ReportTone Amber = new("#9A6200", "#FDF1D8");
    public static readonly ReportTone Green = new("#1E6B45", "#DDF0E6");
    public static readonly ReportTone Blue = new("#2C5A85", "#E3EDF6");
    public static readonly ReportTone Ink = new("#1F2A44", "#EEF0F4");
    public static readonly ReportTone Accent = new("#A8432E", "#FBEDE9");
    public static readonly ReportTone Purple = new("#5B3F8C", "#EDE7F6");
    public static readonly ReportTone Grey = new("#4A5266", "#F1EFEA");
}

public sealed record ReportPersonModel
{
    public string FullName { get; init; } = "";
    public string Initials { get; init; } = "";
    public string? PhotoUrl { get; init; }
    public bool HasAccess { get; init; } = true;
}
