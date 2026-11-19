using Dapper;
using Columnist.DataAccess;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Boards;
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
    private readonly IBoardAccessService _boards;

    public DeleteTaskHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        // dbo.DeleteTask says whether the task was there to delete.
        var deleted = await DeleteTaskAsync(taskId.Value, cancellationToken);
        if (!deleted)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        // The task's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }

    private async Task<bool> DeleteTaskAsync(int id, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        parameters.Add("@UserId", _userContext.UserId);
        return await _db.ExecuteAsync("dbo.DeleteTask", parameters, ct);
    }
}