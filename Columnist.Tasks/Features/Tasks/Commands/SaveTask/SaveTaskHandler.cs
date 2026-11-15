using Dapper;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using MediatR;
using System.Data;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveTask;

public sealed class SaveTaskHandler : IRequestHandler<SaveTaskCommand, ApiResponse<bool>>
{
    private const int AssigneeNotFoundResult = 3;
    private const int TaskChangedByOtherResult = 4;

    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public SaveTaskHandler(IDbExecutor db, IDmsService dms, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _dms = dms;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(SaveTaskCommand request, CancellationToken ct)
    {
        var taskId = 0;
        if (!request.Task.IsNew)
        {
            var decryptedId = request.Task.Token.Decrypt<int>();
            if (decryptedId is null)
                return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);
            taskId = decryptedId.Value;
        }

        byte[]? rowVersion = null;
        if (!request.Task.IsNew && !string.IsNullOrEmpty(request.RowVersion))
        {
            try
            {
                rowVersion = Convert.FromBase64String(request.RowVersion);
            }
            catch (FormatException)
            {
                return ApiResponse<bool>.Conflict(ErrorMessages.TaskChangedByOther);
            }
        }

        DateTime? originalDueDate = null;
        List<string> oldImages = [];

        if (!request.Task.IsNew)
        {
            var existing = await GetTaskByIdAsync(taskId, ct);
            if (existing is null)
                return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

            originalDueDate = existing.DueDate;

            oldImages = await GetTaskImagesAsync(taskId, ct);
        }

        bool dueDateChanged = request.Task.DueDate?.Date != originalDueDate?.Date;
        if (dueDateChanged && request.Task.DueDate is not null && request.Task.DueDate.Value.Date < DateTime.Today)
            return ApiResponse<bool>.BadRequest(ErrorMessages.DueDateInPast, nameof(TaskFormDto.DueDate));

        var columnId = request.Task.ColumnToken.Decrypt<int>();
        if (columnId is null)
            return ApiResponse<bool>.BadRequest(ErrorMessages.ChooseStatus, nameof(TaskFormDto.ColumnToken));

        var assigneeIds = new List<int>();
        foreach (var token in request.Task.AssigneeTokens ?? [])
        {
            var assigneeId = token.Decrypt<int>();
            if (assigneeId is null)
                return ApiResponse<bool>.BadRequest(ErrorMessages.AssigneeNotFound, nameof(TaskFormDto.AssigneeTokens));
            if (!assigneeIds.Contains(assigneeId.Value))
                assigneeIds.Add(assigneeId.Value);
        }

        // New files are deleted automatically on any early return or exception, unless committed.
        using var uploads = _dms.BeginUpload(DmsFolder.Task);
        foreach (var image in request.Task.Images ?? [])
        {
            var upload = await uploads.AddAsync(image, ct);
            if (!upload.Success)
                return ApiResponse<bool>.BadRequest(upload.Error!, nameof(TaskFormDto.Images));
        }

        var result = await SaveTaskAsync(taskId, columnId.Value, assigneeIds, rowVersion, request, uploads.FileNames, ct);
        if (result == AssigneeNotFoundResult)
            return ApiResponse<bool>.BadRequest(ErrorMessages.AssigneeNotFound, nameof(TaskFormDto.AssigneeTokens));
        if (result == TaskChangedByOtherResult)
            return ApiResponse<bool>.Conflict(ErrorMessages.TaskChangedByOther);
        if (result != 0)
            return ApiResponse<bool>.BadRequest(ErrorMessages.SaveFailed);

        uploads.Commit();

        if (request.Task.RemoveImages)
            _dms.Delete(DmsFolder.Task, oldImages);

        var boardId = await GetBoardIdByColumnAsync(columnId.Value, ct);
        if (boardId is not null)
            await _notifier.BoardChangedAsync(boardId.Value, ct);
        return ApiResponse<bool>.Success(true);
    }

    private async Task<int?> GetBoardIdByColumnAsync(int columnId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@ColumnId", columnId);
        return await _db.QuerySingleOrDefaultAsync<int?>("dbo.GetBoardIdByColumn", parameters, ct);
    }

    private async Task<TaskDetailsDto?> GetTaskByIdAsync(int id, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await _db.QuerySingleOrDefaultAsync<TaskDetailsDto>("dbo.GetTaskById", parameters, ct);
    }

    private async Task<List<string>> GetTaskImagesAsync(int taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        var images = await _db.QueryAsync<string>("dbo.GetImagesByTaskId", parameters, ct);
        return images.ToList();
    }

    // Returns the procedure's return value: 0 = saved, 3 = an assignee doesn't exist, 4 = changed by someone else since RowVersion.
    private async Task<int> SaveTaskAsync(int taskId, int columnId, IReadOnlyList<int> assigneeIds, byte[]? rowVersion, SaveTaskCommand request, IReadOnlyList<string> imageNames, CancellationToken ct)
    {
        var images = new DataTable();
        images.Columns.Add("FileName", typeof(string));
        foreach (var fileName in imageNames)
            images.Rows.Add(fileName);

        // dbo.UserIdList: UserId.
        var assignees = new DataTable();
        assignees.Columns.Add("UserId", typeof(int));
        foreach (var userId in assigneeIds)
            assignees.Rows.Add(userId);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId);
        parameters.Add("@ColumnId", columnId);
        parameters.Add("@Title", request.Task.Title);
        parameters.Add("@Description", request.Task.Description);
        parameters.Add("@AssigneeIds", assignees.AsTableValuedParameter("dbo.UserIdList"));
        parameters.Add("@DueDate", request.Task.DueDate);
        parameters.Add("@RemoveImages", request.Task.RemoveImages);
        parameters.Add("@Images", images.AsTableValuedParameter("dbo.ImageNameList"));
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@RowVersion", rowVersion, DbType.Binary, size: 8);

        await _db.ExecuteAsync("dbo.SaveTaskWithImages", parameters, ct);
        return parameters.Get<int>("@ReturnValue");
    }
}
