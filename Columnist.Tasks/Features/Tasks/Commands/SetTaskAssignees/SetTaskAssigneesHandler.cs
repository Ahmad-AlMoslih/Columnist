using Dapper;
using Columnist.DataAccess;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Boards;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using MediatR;
using System.Data;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands.SetTaskAssignees;

public sealed class SetTaskAssigneesHandler : IRequestHandler<SetTaskAssigneesCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;
    private readonly IBoardAccessService _boards;

    public SetTaskAssigneesHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<bool>> Handle(SetTaskAssigneesCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);
        if (!TaskFieldSave.TryParseRowVersion(request.RowVersion, out var rowVersion))
            return ApiResponse<bool>.Conflict(ErrorMessages.TaskChangedByOther);

        // dbo.UserIdList: UserId.
        var assignees = new DataTable();
        assignees.Columns.Add("UserId", typeof(int));
        var seen = new HashSet<int>();
        foreach (var token in request.AssigneeTokens ?? [])
        {
            var userId = token.Decrypt<int>();
            if (userId is null)
                return ApiResponse<bool>.BadRequest(ErrorMessages.AssigneeNotFound, nameof(request.AssigneeTokens));
            if (seen.Add(userId.Value))
                assignees.Rows.Add(userId.Value);
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        parameters.Add("@AssigneeIds", assignees.AsTableValuedParameter("dbo.UserIdList"));
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@RowVersion", rowVersion, DbType.Binary, size: 8);
        if (!await _db.ExecuteAsync("dbo.SetTaskAssignees", parameters, cancellationToken))
            return TaskFieldSave.Failure(parameters.Get<int>("@ReturnValue"), nameof(request.AssigneeTokens));

        // The task's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}
