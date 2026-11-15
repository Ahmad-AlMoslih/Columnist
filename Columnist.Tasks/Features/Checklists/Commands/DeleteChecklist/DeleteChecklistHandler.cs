using Columnist.Auth.Security.CurrentUser;
using Columnist.DataAccess;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Commands.DeleteChecklist;

public sealed class DeleteChecklistHandler : IRequestHandler<DeleteChecklistCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public DeleteChecklistHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteChecklistCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.TaskToken.Decrypt<int>();
        var checklistId = request.ChecklistToken.Decrypt<int>();
        if (taskId is null || checklistId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.ChecklistNotFound);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", checklistId.Value);
        parameters.Add("@TaskId", taskId.Value);
        parameters.Add("@UserId", _userContext.UserId);

        var deleted = await _db.ExecuteAsync("dbo.DeleteChecklist", parameters, cancellationToken);
        if (!deleted)
            return ApiResponse<bool>.NotFound(ErrorMessages.ChecklistNotFound);

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
