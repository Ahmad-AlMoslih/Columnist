using Dapper;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using MediatR;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public DeleteTaskHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var task = await GetTaskByIdAsync(taskId.Value, cancellationToken);
        if (task is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var deleted = await DeleteTaskAsync(taskId.Value, cancellationToken);
        if (!deleted)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var boardId = await GetBoardIdByColumnAsync(task.ColumnId, cancellationToken);
        if (boardId is not null)
            await _notifier.BoardChangedAsync(boardId.Value, cancellationToken);
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

    private async Task<bool> DeleteTaskAsync(int id, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        parameters.Add("@UserId", _userContext.UserId);
        return await _db.ExecuteAsync("dbo.DeleteTask", parameters, ct);
    }
}