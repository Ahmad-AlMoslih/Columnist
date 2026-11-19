using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Reports.Queries.GetBoardDashboard.Models;
using Columnist.Tasks.Features.Reports.Queries.GetDashboardCard;
using Columnist.Tasks.Features.Reports.Shared;
using Columnist.Tasks.Features.Reports.Shared.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardDashboard;

// The 8 cards with their numbers only (dbo.GetBoardDashboardCounts, one row); a card's rows load when its "View all"
// opens (GetDashboardCardQuery, by the same rules).
public sealed class GetBoardDashboardHandler : IRequestHandler<GetBoardDashboardQuery, ApiResponse<GetBoardDashboardModel>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardAccessService _boards;

    public GetBoardDashboardHandler(IDbExecutor db, IUserContext userContext, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _boards = boards;
    }

    public async Task<ApiResponse<GetBoardDashboardModel>> Handle(GetBoardDashboardQuery request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<GetBoardDashboardModel>.NotFound(ErrorMessages.BoardNotFound);

        var now = DateTime.Now;   // the DB stores local times (SYSDATETIME), as in BoardReportData
        var countParameters = new DynamicParameters();
        countParameters.Add("@BoardId", boardId.Value);
        countParameters.Add("@UserId", _userContext.UserId);
        countParameters.Add("@Today", now.Date);
        countParameters.Add("@Since", now.AddDays(-ActivityRules.Days));
        countParameters.Add("@CreatedTake", GetDashboardCardHandler.RecentlyCreatedCount);
        countParameters.Add("@ActivityTake", GetDashboardCardHandler.RecentActivityCount);
        var counts = (await _db.QueryAsync<BoardDashboardCountsDto>("dbo.GetBoardDashboardCounts", countParameters, cancellationToken)).Single();

        var nameParameters = new DynamicParameters();
        nameParameters.Add("@UserId", _userContext.UserId);
        var boardName = (await _db.QueryAsync<OptionDto>("dbo.GetUserBoards", nameParameters, cancellationToken)).FirstOrDefault(b => b.Id == boardId.Value)?.Name ?? "";

        static ReportCardModel Card(string key, string label, string note, ReportTone tone, int count) =>
            new() { Key = key, Label = label, Note = note, Tone = tone, Count = count };

        var cards = new List<ReportCardModel>
        {
            Card("overdue", "Overdue", "Past their due date and not done", ReportTone.Red, counts.Overdue),
            Card("week", "Due this week", "Due in the next 7 days", ReportTone.Amber, counts.Week),
            Card("mine", "Assigned to me", "Open tasks you are on", ReportTone.Ink, counts.Mine),
            Card("unassigned", "Unassigned", "Open tasks with no one assigned", ReportTone.Accent, counts.Unassigned),
            Card("checklists", "Unfinished checklists", "Open tasks with items still to tick", ReportTone.Blue, counts.Checklists),
            Card("completed", "Recently completed", "Moved to Done in the last 7 days", ReportTone.Green, counts.Completed),
            Card("created", "Recently created", "The newest tasks on this board", ReportTone.Purple, counts.Created),
            Card("activity", "Recent activity", "The latest changes on this board", ReportTone.Accent, counts.Activity)
        };

        return ApiResponse<GetBoardDashboardModel>.Success(new GetBoardDashboardModel
        {
            BoardToken = request.BoardToken,
            BoardName = boardName,
            Cards = cards,
            CanSeeDeleted = await _boards.HasPermissionAsync(boardId.Value, Permission.DeletedTasksRead, cancellationToken),
            CanSeeLogs = await _boards.HasPermissionAsync(boardId.Value, Permission.LogsRead, cancellationToken),
            CanSeeReports = await _boards.HasPermissionAsync(boardId.Value, Permission.ReportsRead, cancellationToken),
            CanManage = await _boards.IsOwnerAsync(boardId.Value, cancellationToken)
        });
    }
}
