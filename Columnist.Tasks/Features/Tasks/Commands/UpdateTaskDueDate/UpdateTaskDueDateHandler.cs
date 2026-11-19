using Dapper;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Boards;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using MediatR;
using System.Data;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands.UpdateTaskDueDate;

public sealed class UpdateTaskDueDateHandler : IRequestHandler<UpdateTaskDueDateCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;
    private readonly IBoardAccessService _boards;

    public UpdateTaskDueDateHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateTaskDueDateCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);
        if (!TaskFieldSave.TryParseRowVersion(request.RowVersion, out var rowVersion))
            return ApiResponse<bool>.Conflict(ErrorMessages.TaskChangedByOther);

        var existing = await GetTaskByIdAsync(taskId.Value, cancellationToken);
        if (existing is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        // "Can't be in the past" only when the date changes, so an overdue task can still be saved as it is.
        var dueDate = request.DueDate?.Date;
        if (dueDate != existing.DueDate?.Date && dueDate is not null && dueDate.Value < DateTime.Today)
            return ApiResponse<bool>.BadRequest(ErrorMessages.DueDateInPast, nameof(TaskFormDto.DueDate));

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        parameters.Add("@DueDate", dueDate, DbType.Date);
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@RowVersion", rowVersion, DbType.Binary, size: 8);
        if (!await _db.ExecuteAsync("dbo.UpdateTaskDueDate", parameters, cancellationToken))
            return TaskFieldSave.Failure(parameters.Get<int>("@ReturnValue"));

        // The task's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }

    private async Task<TaskDetailsDto?> GetTaskByIdAsync(int id, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await _db.QuerySingleOrDefaultAsync<TaskDetailsDto>("dbo.GetTaskById", parameters, ct);
    }
}
