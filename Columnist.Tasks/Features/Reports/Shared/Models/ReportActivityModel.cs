namespace Columnist.Tasks.Features.Reports.Shared.Models;

// A dashboard "Recent activity" entry: "<actor> <text> · <time>". TaskToken = its task, when that task still exists.
public sealed record ReportActivityModel
{
    public ReportPersonModel Actor { get; init; } = new();
    public string Text { get; init; } = "";
    public string Time { get; init; } = "";
    public string? TaskToken { get; init; }
}
