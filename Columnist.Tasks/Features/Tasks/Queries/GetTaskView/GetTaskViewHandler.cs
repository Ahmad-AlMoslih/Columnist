using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Checklists.Models;
using Columnist.Tasks.Features.Tasks.Models;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskView.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskView;

public sealed class GetTaskViewHandler : IRequestHandler<GetTaskViewQuery, ApiResponse<GetTaskViewModel>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserPhotoService _photos;
    private readonly IBoardAccessService _boards;
    private readonly IUserContext _userContext;
    private readonly DmsOptions _dmsOptions;

    public GetTaskViewHandler(IDbExecutor db, IDmsService dms, IUserPhotoService photos, IBoardAccessService boards, IUserContext userContext, DmsOptions dmsOptions)
    {
        _db = db;
        _dms = dms;
        _photos = photos;
        _boards = boards;
        _userContext = userContext;
        _dmsOptions = dmsOptions;
    }

    public async Task<ApiResponse<GetTaskViewModel>> Handle(GetTaskViewQuery request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<GetTaskViewModel>.NotFound(ErrorMessages.TaskNotFound);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        var task = await _db.QuerySingleOrDefaultAsync<TaskViewDto>("dbo.GetTaskDetails", parameters, cancellationToken);
        if (task is null)
            return ApiResponse<GetTaskViewModel>.NotFound(ErrorMessages.TaskNotFound);

        var imageParameters = new DynamicParameters();
        imageParameters.Add("@TaskId", taskId.Value);
        var images = await _db.QueryAsync<TaskImageDto>("dbo.GetImagesByTaskId", imageParameters, cancellationToken);

        var assignees = await GetTaskAssigneesAsync(taskId.Value, null, cancellationToken);

        var checklistParameters = new DynamicParameters();
        checklistParameters.Add("@TaskId", taskId.Value);
        var checklists = await _db.QueryAsync<TaskChecklistRowDto>("dbo.GetTaskChecklists", checklistParameters, cancellationToken);

        var historyParameters = new DynamicParameters();
        historyParameters.Add("@TaskId", taskId.Value);
        var history = await _db.QueryAsync<AuditLogRowDto>("dbo.GetTaskHistory", historyParameters, cancellationToken);

        var photos = await _photos.GetUrlsAsync(cancellationToken);

        // The task's board, resolved once (cached since the pipeline's check).
        var boardIds = await _boards.ResolveAsync(new BoardScope(TaskToken: request.Token), cancellationToken);
        var canWrite = boardIds is [var writeBoard] && await _boards.HasPermissionAsync(writeBoard, Permission.TasksWrite, cancellationToken);
        var canDelete = boardIds is [var deleteBoard] && await _boards.HasPermissionAsync(deleteBoard, Permission.TasksDelete, cancellationToken);

        // The editing lists are only needed by people who can edit.
        IReadOnlyList<TaskColumnOptionModel> columns = [];
        IReadOnlyList<TaskAssigneeModel> members = [];
        if (canWrite)
        {
            var columnRows = await GetColumnOptionsAsync(task.BoardId, cancellationToken);
            columns = columnRows
                .Select(c => new TaskColumnOptionModel(c.ColumnId.Encrypt(), c.ColumnName, c.StatusName, c.StatusColor, c.ColumnId == task.ColumnId))
                .ToList();

            var users = await GetUserOptionsAsync(task.BoardId, taskId.Value, cancellationToken);
            members = users
                .Select(u => TaskAssigneeModel.From(new TaskAssigneeDto { UserId = u.Id, FullName = u.Name, HasAccess = u.HasAccess })
                             with { PhotoUrl = photos.GetValueOrDefault(u.Id), Key = u.Id.ToStableKey("user") })
                .ToList();
        }

        return ApiResponse<GetTaskViewModel>.Success(new GetTaskViewModel
        {
            Token = request.Token,
            CanWrite = canWrite,
            CanDelete = canDelete,
            Key = taskId.Value.ToStableKey("task"),
            Title = task.Title,
            Description = task.Description,
            BoardName = task.BoardName,
            ColumnName = task.ColumnName,
            StatusColor = task.StatusColor,
            StatusName = task.StatusName,
            Assignees = assignees.Select(a => TaskAssigneeModel.From(a) with { PhotoUrl = photos.GetValueOrDefault(a.UserId), Key = a.UserId.ToStableKey("user") }).ToList(),
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            CreatedByName = task.CreatedByName,
            UpdatedAt = task.UpdatedAt,
            UpdatedByName = task.UpdatedByName,
            Images = images.Select(i => new TaskImageModel(i.Id.Encrypt(), i.Id.ToStableKey("image"), _dms.GetUrl(DmsFolder.Task, i.FileName)!)).ToList(),
            Checklists = ChecklistModel.FromRows(checklists),
            RowVersion = task.RowVersion is null ? null : Convert.ToBase64String(task.RowVersion),
            BoardToken = task.BoardId.Encrypt(),
            ColumnToken = task.ColumnId.Encrypt(),
            Columns = columns,
            Members = members,
            UploadMaxFiles = _dmsOptions.MaxFilesPerUpload,
            UploadMaxBytes = _dmsOptions.MaxFileSizeBytes,
            UploadAllowedTypes = _dmsOptions.AllowedExtensions,
            History = history
                .OrderByDescending(h => h.CreatedAt)
                .ThenByDescending(h => h.Id)
                .Select(h => new AuditEntryModel
                {
                    ActorName = h.ActorName,
                    ActorInitials = GetInitials(h.ActorName),
                    PhotoUrl = photos.GetValueOrDefault(h.ActorId),
                    Action = h.Action,
                    Category = h.Category,
                    TargetName = h.TargetName,
                    Field = h.Field,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    CreatedAt = h.CreatedAt
                })
                .ToList()
        });
    }

    // First letters of the first two words of the name, uppercase.
    private static string GetInitials(string fullName) =>
        string.Concat(fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0])));

    private async Task<IReadOnlyList<TaskAssigneeDto>> GetTaskAssigneesAsync(int? taskId, int? boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        parameters.Add("@BoardId", boardId);
        return await _db.QueryAsync<TaskAssigneeDto>("dbo.GetTaskAssignees", parameters, ct);
    }

    private async Task<IReadOnlyList<ColumnOptionDto>> GetColumnOptionsAsync(int boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        return await _db.QueryAsync<ColumnOptionDto>("dbo.GetBoardColumnOptions", parameters, ct);
    }

    // Board members, the team owner and the task's current assignees (dbo.GetUsersForDropdown).
    private async Task<IReadOnlyList<UserOptionDto>> GetUserOptionsAsync(int boardId, int taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        parameters.Add("@TaskId", taskId);
        parameters.Add("@UserId", _userContext.UserId);
        return await _db.QueryAsync<UserOptionDto>("dbo.GetUsersForDropdown", parameters, ct);
    }
}
