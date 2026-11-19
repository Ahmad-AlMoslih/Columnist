using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Boards;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Commands.SaveChecklist;

public sealed class SaveChecklistHandler : IRequestHandler<SaveChecklistCommand, ApiResponse<string>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;
    private readonly IBoardAccessService _boards;

    public SaveChecklistHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<string>> Handle(SaveChecklistCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.TaskToken.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<string>.NotFound(ErrorMessages.ChecklistNotFound);

        var checklistId = 0;
        if (!string.IsNullOrEmpty(request.ChecklistToken))
        {
            var decrypted = request.ChecklistToken.Decrypt<int>();
            if (decrypted is null)
                return ApiResponse<string>.NotFound(ErrorMessages.ChecklistNotFound);
            checklistId = decrypted.Value;
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Id", checklistId);
        parameters.Add("@TaskId", taskId.Value);
        parameters.Add("@Title", request.Title.Trim());
        parameters.Add("@UserId", _userContext.UserId);

        var result = await _db.QuerySingleOrDefaultAsync<SaveChecklistResultDto>("dbo.SaveChecklist", parameters, cancellationToken);
        if (result is not { Result: 0, Id: not null })
            return ApiResponse<string>.NotFound(ErrorMessages.ChecklistNotFound);

        // The task's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, cancellationToken);
        return ApiResponse<string>.Success(result.Id.Value.Encrypt());
    }
}
