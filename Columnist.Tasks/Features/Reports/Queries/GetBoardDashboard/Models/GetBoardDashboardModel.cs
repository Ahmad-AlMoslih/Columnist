using Columnist.Tasks.Features.Reports.Shared.Models;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardDashboard.Models;

public sealed record GetBoardDashboardModel
{
    public string BoardToken { get; init; } = "";
    public string BoardName { get; init; } = "";
    public IReadOnlyList<ReportCardModel> Cards { get; init; } = [];   // the numbers only, no rows

    // The board menu's links.
    public bool CanSeeDeleted { get; init; }
    public bool CanSeeLogs { get; init; }
    public bool CanSeeReports { get; init; }
    public bool CanManage { get; init; }   // Super Admin of the board's team
}
