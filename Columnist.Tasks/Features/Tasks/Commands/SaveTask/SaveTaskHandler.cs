using Dapper;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Authorization;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using MediatR;
using System.Data;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveTask;

// Returns the saved task's token.
public sealed class SaveTaskHandler : IRequestHandler<SaveTaskCommand, ApiResponse<string>>
{
    private const int AssigneeNotFoundResult = 3;
    private const int TaskChangedByOtherResult = 4;

    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;
    private readonly IBoardAccessService _boards;

    public SaveTaskHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<string>> Handle(SaveTaskCommand request, CancellationToken ct)
    {
        var taskId = 0;
        if (!request.Task.IsNew)
        {
            var decryptedId = request.Task.Token.Decrypt<int>();
            if (decryptedId is null)
                return ApiResponse<string>.NotFound(ErrorMessages.TaskNotFound);
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
                return ApiResponse<string>.Conflict(ErrorMessages.TaskChangedByOther);
            }
        }

        DateTime? originalDueDate = null;

        if (!request.Task.IsNew)
        {
            var existing = await GetTaskByIdAsync(taskId, ct);
            if (existing is null)
                return ApiResponse<string>.NotFound(ErrorMessages.TaskNotFound);

            originalDueDate = existing.DueDate;
        }

        bool dueDateChanged = request.Task.DueDate?.Date != originalDueDate?.Date;
        if (dueDateChanged && request.Task.DueDate is not null && request.Task.DueDate.Value.Date < DateTime.Today)
            return ApiResponse<string>.BadRequest(ErrorMessages.DueDateInPast, nameof(TaskFormDto.DueDate));

        var columnId = request.Task.ColumnToken.Decrypt<int>();
        if (columnId is null)
            return ApiResponse<string>.BadRequest(ErrorMessages.ChooseStatus, nameof(TaskFormDto.ColumnToken));

        var assigneeIds = new List<int>();
        foreach (var token in request.Task.AssigneeTokens ?? [])
        {
            var assigneeId = token.Decrypt<int>();
            if (assigneeId is null)
                return ApiResponse<string>.BadRequest(ErrorMessages.AssigneeNotFound, nameof(TaskFormDto.AssigneeTokens));
            if (!assigneeIds.Contains(assigneeId.Value))
                assigneeIds.Add(assigneeId.Value);
        }

        var (result, savedId) = await SaveTaskAsync(taskId, columnId.Value, assigneeIds, rowVersion, request, ct);
        if (result == AssigneeNotFoundResult)
            return ApiResponse<string>.BadRequest(ErrorMessages.AssigneeNotFound, nameof(TaskFormDto.AssigneeTokens));
        if (result == TaskChangedByOtherResult)
            return ApiResponse<string>.Conflict(ErrorMessages.TaskChangedByOther);
        if (result != 0 || savedId is null)
            return ApiResponse<string>.BadRequest(ErrorMessages.SaveFailed);

        // The column's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(new BoardScope(ColumnToken: request.Task.ColumnToken), ct);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, ct);
        return ApiResponse<string>.Success(savedId.Value.Encrypt());
    }


    private async Task<TaskDetailsDto?> GetTaskByIdAsync(int id, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await _db.QuerySingleOrDefaultAsync<TaskDetailsDto>("dbo.GetTaskById", parameters, ct);
    }

    // Returns the procedure's return value (0 = saved, 3 = an assignee doesn't exist, 4 = changed by someone else since RowVersion)
    // and the saved task's id.
    private async Task<(int Result, int? Id)> SaveTaskAsync(int taskId, int columnId, IReadOnlyList<int> assigneeIds, byte[]? rowVersion, SaveTaskCommand request, CancellationToken ct)
    {
        // dbo.ImageNameList: FileName. Images are added in the task view (AddTaskImages), so none here.
        var images = new DataTable();
        images.Columns.Add("FileName", typeof(string));

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
        parameters.Add("@RemoveImages", false);
        parameters.Add("@Images", images.AsTableValuedParameter("dbo.ImageNameList"));
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@RowVersion", rowVersion, DbType.Binary, size: 8);
        parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _db.ExecuteAsync("dbo.SaveTaskWithImages", parameters, ct);
        return (parameters.Get<int>("@ReturnValue"), parameters.Get<int?>("@NewId"));
    }
}
