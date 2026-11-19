using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Models;
using Dapper;

namespace Columnist.Tasks.Features.Reports.Shared;

// Everything the dashboard, the report and the CSV export read about one board: its columns in board order,
// its tasks (with assignees, checklist counts and history) and its people (board members and the team owner).
public sealed class BoardReportData
{
    public string BoardName { get; private init; } = "";
    public IReadOnlyList<ColumnOptionDto> Columns { get; private init; } = [];
    public IReadOnlyList<ReportTask> Tasks { get; private init; } = [];
    public IReadOnlyList<UserOptionDto> People { get; private init; } = [];
    public IReadOnlyDictionary<int, string> Photos { get; private init; } = new Dictionary<int, string>();
    public DateTime Now { get; private init; }   // the DB stores local times (SYSDATETIME)
    public DateOnly Today => DateOnly.FromDateTime(Now);

    public static async Task<BoardReportData> LoadAsync(IDbExecutor db, IUserPhotoService photoService, int boardId, int? userId, CancellationToken ct)
    {
        var boardParameters = new DynamicParameters();
        boardParameters.Add("@BoardId", boardId);

        var columns = await db.QueryAsync<ColumnOptionDto>("dbo.GetBoardColumnOptions", boardParameters, ct);
        var tasks = await db.QueryAsync<TaskItemDto>("dbo.GetAllTasksWithDetails", boardParameters, ct);
        var checklists = (await db.QueryAsync<BoardChecklistCountDto>("dbo.GetBoardChecklistCounts", boardParameters, ct)).ToDictionary(c => c.TaskId);
        var history = (await db.QueryAsync<ReportTaskHistoryDto>("dbo.GetBoardTaskHistory", boardParameters, ct)).ToDictionary(h => h.TaskId);

        var assigneeParameters = new DynamicParameters();
        assigneeParameters.Add("@TaskId", null);
        assigneeParameters.Add("@BoardId", boardId);
        var assigneeRows = await db.QueryAsync<TaskAssigneeDto>("dbo.GetTaskAssignees", assigneeParameters, ct);

        var peopleParameters = new DynamicParameters();
        peopleParameters.Add("@BoardId", boardId);
        peopleParameters.Add("@TaskId", null);
        peopleParameters.Add("@UserId", userId);
        var people = await db.QueryAsync<UserOptionDto>("dbo.GetUsersForDropdown", peopleParameters, ct);

        var nameParameters = new DynamicParameters();
        nameParameters.Add("@UserId", userId);
        var boardName = (await db.QueryAsync<OptionDto>("dbo.GetUserBoards", nameParameters, ct)).FirstOrDefault(b => b.Id == boardId)?.Name ?? "";

        var photos = await photoService.GetUrlsAsync(ct);
        var assignees = assigneeRows.ToLookup(a => a.TaskId);

        return new BoardReportData
        {
            BoardName = boardName,
            Columns = columns,
            People = people,
            Photos = photos,
            Now = DateTime.Now,
            Tasks = tasks
                .Select(t =>
                {
                    var taskHistory = history.GetValueOrDefault(t.Id);
                    var counts = checklists.GetValueOrDefault(t.Id);
                    return new ReportTask
                    {
                        Id = t.Id,
                        Token = t.Id.Encrypt(),
                        Title = t.Title,
                        ColumnId = t.ColumnId,
                        ColumnName = t.ColumnName,
                        StatusName = t.StatusName,
                        StatusColor = t.StatusColor,
                        IsDone = t.StatusName.Equals("Done", StringComparison.OrdinalIgnoreCase),
                        DueDate = t.DueDate is { } due ? DateOnly.FromDateTime(due) : null,
                        CreatedAt = t.CreatedAt,
                        Assignees = assignees[t.Id].Select(a => TaskAssigneeModel.From(a) with { PhotoUrl = photos.GetValueOrDefault(a.UserId) }).ToList(),
                        AssigneeIds = assignees[t.Id].Select(a => a.UserId).ToList(),
                        ChecklistDone = counts?.Done ?? 0,
                        ChecklistTotal = counts?.Total ?? 0,
                        ColumnEnteredAt = taskHistory?.ColumnEnteredAt ?? t.CreatedAt,
                        ColumnEnteredByName = taskHistory?.ColumnEnteredByName,
                        LastUpdatedAt = taskHistory?.LastUpdatedAt ?? t.CreatedAt
                    };
                })
                .ToList()
        };
    }
}

// A task with what the reports work out from its history.
public sealed record ReportTask
{
    public int Id { get; init; }
    public string Token { get; init; } = "";
    public string Title { get; init; } = "";
    public int ColumnId { get; init; }
    public string ColumnName { get; init; } = "";
    public string StatusName { get; init; } = "";
    public string StatusColor { get; init; } = "";
    public bool IsDone { get; init; }   // in a column whose status is Done
    public DateOnly? DueDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<TaskAssigneeModel> Assignees { get; init; } = [];
    public IReadOnlyList<int> AssigneeIds { get; init; } = [];
    public int ChecklistDone { get; init; }
    public int ChecklistTotal { get; init; }

    // The latest TaskMoved entry (moves are logged only when the column changes), else the creation.
    public DateTime ColumnEnteredAt { get; init; }
    public string? ColumnEnteredByName { get; init; }
    // The task's latest audit entry of any kind, else the creation.
    public DateTime LastUpdatedAt { get; init; }

    // A Done task was completed when it came into its Done column, by whoever moved it there.
    public DateTime? CompletedAt => IsDone ? ColumnEnteredAt : null;
    public DateOnly? CompletedOn => CompletedAt is { } at ? DateOnly.FromDateTime(at) : null;
    public string? CompletedByName => IsDone ? ColumnEnteredByName : null;

    public int DaysInColumn(DateOnly today) => Math.Max(0, today.DayNumber - DateOnly.FromDateTime(ColumnEnteredAt).DayNumber);
    public int DaysSinceUpdate(DateOnly today) => Math.Max(0, today.DayNumber - DateOnly.FromDateTime(LastUpdatedAt).DayNumber);
    public int DaysSinceCreated(DateOnly today) => Math.Max(0, today.DayNumber - DateOnly.FromDateTime(CreatedAt).DayNumber);
    public bool IsOverdue(DateOnly today) => !IsDone && DueDate < today;

    // Done: done date − due date. Open and overdue: today − due date. Otherwise null (not late).
    public int? DaysLate(DateOnly today) => DueDate switch
    {
        null => null,
        { } due when IsDone => CompletedOn > due ? CompletedOn!.Value.DayNumber - due.DayNumber : null,
        { } due => due < today ? today.DayNumber - due.DayNumber : null
    };
}
