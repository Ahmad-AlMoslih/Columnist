namespace Columnist.Tasks.Features.Reports.Shared.Models;

// A row of the report's "Activity per person": the person, "12 changes" and "5 moves · 4 edits · 3 comments".
public sealed record ReportPersonRowModel
{
    public ReportPersonModel Person { get; init; } = new();
    public int Changes { get; init; }
    public string Breakdown { get; init; } = "";
}
