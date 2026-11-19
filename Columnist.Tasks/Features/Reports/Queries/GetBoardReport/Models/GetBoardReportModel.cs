using Columnist.Contracts.Dtos;
using Columnist.Tasks.Features.Reports.Shared.Models;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardReport.Models;

public sealed record GetBoardReportModel
{
    public string BoardToken { get; init; } = "";
    public string BoardName { get; init; } = "";

    // The filter panel.
    public string? Search { get; init; }
    public string? From { get; init; }   // yyyy-MM-dd
    public string? To { get; init; }     // yyyy-MM-dd
    public IReadOnlyList<TokenOptionDto> Assignees { get; init; } = [];   // "Anyone" is the empty option
    public IReadOnlyList<ReportColumnOptionModel> Columns { get; init; } = [];
    public IReadOnlyList<KeyValuePair<string, string?>> Query { get; init; } = [];   // the filter's query string values

    public IReadOnlyList<ReportGroupModel> Groups { get; init; } = [];

    // The board menu's links.
    public bool CanSeeDeleted { get; init; }
    public bool CanSeeLogs { get; init; }
    public bool CanSeeReports { get; init; }
    public bool CanManage { get; init; }   // Super Admin of the board's team
}

public sealed record ReportGroupModel(string Title, IReadOnlyList<ReportCardModel> Cards);

public sealed record ReportColumnOptionModel
{
    public string Token { get; init; } = "";
    public string Name { get; init; } = "";
    public string? Color { get; init; }   // status colour, null = neutral
    public bool Selected { get; init; }
}
