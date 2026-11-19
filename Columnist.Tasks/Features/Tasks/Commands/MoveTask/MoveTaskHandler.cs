using Dapper;
using Columnist.DataAccess;
using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using MediatR;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using System.Data;

namespace Columnist.Tasks.Features.Tasks.Commands.MoveTask;

public sealed class MoveTaskHandler : IRequestHandler<MoveTaskCommand, ApiResponse<bool>>
{
    private const int TaskChangedByOtherResult = 4;

    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;
    private readonly IBoardAccessService _boards;

    public MoveTaskHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<bool>> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var columnId = request.ColumnToken.Decrypt<int>();
        if (columnId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        byte[]? rowVersion = null;
        if (!string.IsNullOrEmpty(request.RowVersion))
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

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        parameters.Add("@ColumnId", columnId.Value);
        parameters.Add("@Position", request.Position);
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@RowVersion", rowVersion, DbType.Binary, size: 8);
        var moved = await _db.ExecuteAsync("dbo.MoveTask", parameters, cancellationToken);
        if (!moved)
        {
            // 4 = changed by someone else since RowVersion; anything else = task or column not found.
            return parameters.Get<int>("@ReturnValue") == TaskChangedByOtherResult
                ? ApiResponse<bool>.Conflict(ErrorMessages.TaskChangedByOther)
                : ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);
        }

        // The column's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(new BoardScope(ColumnToken: request.ColumnToken), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}
