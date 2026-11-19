using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Boards;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Commands.RestoreTask;

public sealed class RestoreTaskHandler : IRequestHandler<RestoreTaskCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;
    private readonly IBoardAccessService _boards;

    public RestoreTaskHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<bool>> Handle(RestoreTaskCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        parameters.Add("@UserId", _userContext.UserId);

        var restored = await _db.ExecuteAsync("dbo.RestoreTask", parameters, cancellationToken);
        if (!restored)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        // The task's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}
