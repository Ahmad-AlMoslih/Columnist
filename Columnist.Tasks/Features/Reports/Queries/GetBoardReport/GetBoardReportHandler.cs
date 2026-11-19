using System.Globalization;
using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Reports.Queries.GetBoardReport.Models;
using Columnist.Tasks.Features.Reports.Shared;
using Columnist.Tasks.Features.Reports.Shared.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using MediatR;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardReport;

public sealed class GetBoardReportHandler : IRequestHandler<GetBoardReportQuery, ApiResponse<GetBoardReportModel>>
{
    private const int StaleDays = 14;

    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;
    private readonly IBoardAccessService _boards;

    public GetBoardReportHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _photos = photos;
        _boards = boards;
    }

    public async Task<ApiResponse<GetBoardReportModel>> Handle(GetBoardReportQuery request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<GetBoardReportModel>.NotFound(ErrorMessages.BoardNotFound);

        var data = await BoardReportData.LoadAsync(_db, _photos, boardId.Value, _userContext.UserId, cancellationToken);
        var filter = ReportFilter.Read(request.Filter, data);
        var activity = await BoardActivity.LoadAsync(_db, data, boardId.Value, filter.AssigneeId, cancellationToken);

        var today = data.Today;
        var tasks = data.Tasks.Where(filter.Matches).ToList();
        var open = tasks.Where(t => !t.IsDone).ToList();

        return ApiResponse<GetBoardReportModel>.Success(new GetBoardReportModel
        {
            BoardToken = request.BoardToken,
            BoardName = data.BoardName,
            Search = filter.Search,
            From = filter.From?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            To = filter.To?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Assignees = data.People
                .Select(p => new TokenOptionDto
                {
                    Token = p.Id == filter.AssigneeId ? filter.AssigneeToken! : p.Id.Encrypt(),
                    Name = p.Name,
                    Selected = p.Id == filter.AssigneeId
                })
                .ToList(),
            Columns = data.Columns
                .Select(c => new ReportColumnOptionModel
                {
                    Token = filter.ColumnTokens.GetValueOrDefault(c.ColumnId) ?? c.ColumnId.Encrypt(),
                    Name = c.ColumnName,
                    Color = ReportRows.Color(c.StatusColor),
                    Selected = filter.ColumnTokens.ContainsKey(c.ColumnId)
                })
                .ToList(),
            Query = filter.ToQuery().ToList(),
            Groups =
            [
                new ReportGroupModel("Workload per person", Workload(data, filter, open, today)),
                new ReportGroupModel("Columns", Columns(data, filter, tasks, today)),
                new ReportGroupModel("Progress and health", Health(data, tasks, open, activity, today))
            ],
            CanSeeDeleted = await _boards.HasPermissionAsync(boardId.Value, Permission.DeletedTasksRead, cancellationToken),
            CanSeeLogs = await _boards.HasPermissionAsync(boardId.Value, Permission.LogsRead, cancellationToken),
            CanSeeReports = await _boards.HasPermissionAsync(boardId.Value, Permission.ReportsRead, cancellationToken),
            CanManage = await _boards.IsOwnerAsync(boardId.Value, cancellationToken)
        });
    }

    // One card per board member and the owner; people whose access ran out only while they have open tasks.
    // With the Assignee filter, only that person.
    private static List<ReportCardModel> Workload(BoardReportData data, ReportFilter filter, List<ReportTask> open, DateOnly today) =>
        data.People
            .Where(p => filter.AssigneeId is null || p.Id == filter.AssigneeId)
            .Where(p => p.HasAccess || data.Tasks.Any(t => !t.IsDone && t.AssigneeIds.Contains(p.Id)))
            .Select(p =>
            {
                var theirs = open.Where(t => t.AssigneeIds.Contains(p.Id)).ByDue().ToList();
                var overdue = theirs.Count(t => t.DueDate < today);
                var week = theirs.Count(t => t.DueDate >= today && t.DueDate <= today.AddDays(6));
                var note = theirs.Count == 0
                    ? "No open tasks"
                    : string.Join(" · ", new[]
                    {
                        overdue > 0 ? $"{overdue} overdue" : "Nothing overdue",
                        week > 0 ? $"{week} due this week" : null
                    }.Where(part => part is not null));
                return new ReportCardModel
                {
                    Key = "person",
                    Label = p.Name,
                    Note = note,
                    Tone = ReportTone.Ink,
                    Person = ReportRows.Person(p, data.Photos),
                    Count = theirs.Count,
                    Tasks = theirs.Select(t => ReportRows.Row(t, today)).ToList()
                };
            })
            .ToList();

    // One card per column in board order (only the chosen columns when the Column filter is on).
    private static List<ReportCardModel> Columns(BoardReportData data, ReportFilter filter, List<ReportTask> tasks, DateOnly today) =>
        data.Columns
            .Where(c => filter.ColumnTokens.Count == 0 || filter.ColumnTokens.ContainsKey(c.ColumnId))
            .Select(c =>
            {
                var isDone = c.StatusName.Equals("Done", StringComparison.OrdinalIgnoreCase);
                var inColumn = tasks.Where(t => t.ColumnId == c.ColumnId).ToList();
                var rows = isDone
                    ? inColumn.OrderByDescending(t => t.CompletedAt)
                        .Select(t => ReportRows.Row(t, today, $"Done on {ReportRows.Date(t.CompletedOn!.Value)}"))
                    : inColumn.OrderByDescending(t => t.DaysInColumn(today)).ThenBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
                        .Select(t => ReportRows.Row(t, today, $"In {c.ColumnName} for {ReportRows.Days(t.DaysInColumn(today))}"));
                return new ReportCardModel
                {
                    Key = isDone ? "column-done" : "column",
                    Label = c.ColumnName,
                    Note = isDone ? "Finished tasks"
                        : inColumn.Count == 0 ? "No tasks"
                        : $"Oldest has waited {ReportRows.Days(inColumn.Max(t => t.DaysInColumn(today)))}",
                    Tone = ReportRows.ForStatus(c.StatusColor),
                    Count = inColumn.Count,
                    Tasks = rows.ToList()
                };
            })
            .ToList();

    private static List<ReportCardModel> Health(BoardReportData data, List<ReportTask> tasks, List<ReportTask> open,
        IReadOnlyList<(UserOptionDto Person, ActivityRules.PersonActivity Activity)> activity, DateOnly today)
    {
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var completedThisMonth = tasks
            .Where(t => t.CompletedOn >= monthStart)
            .OrderByDescending(t => t.CompletedAt)
            .Select(t => ReportRows.Row(t, today, $"Completed by {t.CompletedByName ?? "someone"} · {ReportRows.Date(t.CompletedOn!.Value)}") with
            {
                ExtraChip = t.DueDate is null ? null : t.DaysLate(today) is { } late ? $"{ReportRows.Days(late)} late" : "On time",
                ExtraTone = t.DueDate is null ? null : t.DaysLate(today) is null ? ReportTone.Green : ReportTone.Red
            })
            .ToList();

        var completedLate = tasks
            .Where(t => t.IsDone && t.DaysLate(today) is not null)
            .OrderByDescending(t => t.CompletedAt)
            .Select(t => ReportRows.Row(t, today, $"Due {ReportRows.Date(t.DueDate!.Value)} · done {ReportRows.Date(t.CompletedOn!.Value)}"))
            .ToList();

        var stale = open
            .Where(t => t.DaysSinceUpdate(today) >= StaleDays)
            .OrderBy(t => t.LastUpdatedAt)
            .Select(t => ReportRows.Row(t, today, $"Last updated {ReportRows.DaysAgo(t.DaysSinceUpdate(today))}"))
            .ToList();

        var noDueDate = open
            .Where(t => t.DueDate is null)
            .OrderBy(t => t.CreatedAt)
            .Select(t => ReportRows.Row(t, today, $"Created {ReportRows.DaysAgo(t.DaysSinceCreated(today))}"))
            .ToList();

        var checklists = open
            .Where(t => t.ChecklistTotal > 0)
            .OrderBy(t => (double)t.ChecklistDone / t.ChecklistTotal)
            .ThenBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
            .Select(t => ReportRows.Row(t, today, $"{t.ChecklistDone} of {ReportRows.Count(t.ChecklistTotal, "item", "items")}") with
            {
                Progress = (int)Math.Round(100.0 * t.ChecklistDone / t.ChecklistTotal)
            })
            .ToList();

        var people = activity
            .Select(a => new ReportPersonRowModel
            {
                Person = ReportRows.Person(a.Person, data.Photos),
                Changes = a.Activity.Changes,
                Breakdown = a.Activity.Breakdown
            })
            .ToList();

        return
        [
            new ReportCardModel { Key = "completed", Label = "Completed this month", Note = $"Moved to Done since {ReportRows.Date(monthStart)}", Tone = ReportTone.Green, Count = completedThisMonth.Count, Tasks = completedThisMonth },
            new ReportCardModel { Key = "late", Label = "Completed late", Note = "Finished after their due date", Tone = ReportTone.Red, Count = completedLate.Count, Tasks = completedLate },
            new ReportCardModel { Key = "stale", Label = "Stale tasks", Note = $"Open and not updated for {StaleDays}+ days", Tone = ReportTone.Amber, Count = stale.Count, Tasks = stale },
            new ReportCardModel { Key = "nodue", Label = "No due date", Note = "Open tasks without a due date", Tone = ReportTone.Grey, Count = noDueDate.Count, Tasks = noDueDate },
            new ReportCardModel { Key = "checklists", Label = "Checklist progress", Note = "Open tasks with a checklist", Tone = ReportTone.Blue, Count = checklists.Count, Tasks = checklists },
            new ReportCardModel { Key = "activity", Label = "Activity per person", Note = $"Changes in the last {ActivityRules.Days} days", Tone = ReportTone.Accent, Count = people.Sum(p => p.Changes), People = people }
        ];
    }
}
