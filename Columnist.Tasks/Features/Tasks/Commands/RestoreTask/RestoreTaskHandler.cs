using Columnist.Auth.Security.CurrentUser;
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

    public RestoreTaskHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
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

        var boardId = await GetBoardIdByTaskAsync(taskId.Value, cancellationToken);
        if (boardId is not null)
            await _notifier.BoardChangedAsync(boardId.Value, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }

    private async Task<int?> GetBoardIdByTaskAsync(int taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        return await _db.QuerySingleOrDefaultAsync<int?>("dbo.GetBoardIdByTask", parameters, ct);
    }
}
