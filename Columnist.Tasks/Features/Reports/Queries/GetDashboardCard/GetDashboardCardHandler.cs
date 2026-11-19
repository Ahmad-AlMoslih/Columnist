using Columnist.Auth.Authorization;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Reports.Shared;
using Columnist.Tasks.Features.Reports.Shared.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Reports.Queries.GetDashboardCard;

// One Dashboard card's rows (its "View all" popup), by the same rules as its number (dbo.GetBoardDashboardCounts).
// An unknown card is NotFound.
public sealed class GetDashboardCardHandler : IRequestHandler<GetDashboardCardQuery, ApiResponse<ReportCardModel>>
{
    public const int RecentlyCreatedCount = 10;
    public const int RecentActivityCount = 20;

    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetDashboardCardHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<ReportCardModel>> Handle(GetDashboardCardQuery request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<ReportCardModel>.NotFound(ErrorMessages.BoardNotFound);

        if (request.Card == "activity")
            return ApiResponse<ReportCardModel>.Success(new ReportCardModel
            {
                Key = request.Card,
                Activity = await ActivityAsync(boardId.Value, cancellationToken)
            });

        if (request.Card is not ("overdue" or "week" or "mine" or "unassigned" or "checklists" or "completed" or "created"))
            return ApiResponse<ReportCardModel>.NotFound(ErrorMessages.BoardNotFound);

        var data = await BoardReportData.LoadAsync(_db, _photos, boardId.Value, _userContext.UserId, cancellationToken);
        var today = data.Today;
        var open = data.Tasks.Where(t => !t.IsDone);
        var me = _userContext.UserId;
        var since = data.Now.AddDays(-ActivityRules.Days);

        var tasks = request.Card switch
        {
            "overdue" => open.Where(t => t.DueDate < today).ByDue(),
            "week" => open.Where(t => t.DueDate >= today && t.DueDate <= today.AddDays(6)).ByDue(),
            "mine" => open.Where(t => me is { } id && t.AssigneeIds.Contains(id)).ByDue(),
            "unassigned" => open.Where(t => t.AssigneeIds.Count == 0).ByDue(),
            "checklists" => open.Where(t => t.ChecklistTotal > 0 && t.ChecklistDone < t.ChecklistTotal).ByDue(),
            "completed" => data.Tasks.Where(t => t.CompletedAt >= since).OrderByDescending(t => t.CompletedAt),
            _ => data.Tasks.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id).Take(RecentlyCreatedCount)
        };

        return ApiResponse<ReportCardModel>.Success(new ReportCardModel
        {
            Key = request.Card,
            Tasks = tasks.Select(t => ReportRows.Row(t, today)).ToList()
        });
    }

    private async Task<IReadOnlyList<ReportActivityModel>> ActivityAsync(int boardId, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        parameters.Add("@Take", RecentActivityCount);
        var activity = await _db.QueryAsync<ActivityEntryDto>("dbo.GetBoardRecentActivity", parameters, cancellationToken);
        var catalog = await _db.QueryAsync<PermissionDto>("dbo.GetPermissions", ct: cancellationToken);
        var photos = await _photos.GetUrlsAsync(cancellationToken);
        var now = DateTime.Now;   // the DB stores local times (SYSDATETIME)

        return activity
            .Select(a => new ReportActivityModel
            {
                Actor = ReportRows.Person(a.ActorId, a.ActorName, photos),
                Text = ActivityRules.Text(a, a.Action is "PermissionGranted" or "PermissionRemoved" && catalog.FirstOrDefault(p => p.Name == a.NewValue) is { } permission
                    ? PermissionRules.Label(permission)
                    : null),
                Time = ReportRows.Ago(a.CreatedAt, now),
                TaskToken = a.TaskId?.Encrypt()
            })
            .ToList();
    }
}
