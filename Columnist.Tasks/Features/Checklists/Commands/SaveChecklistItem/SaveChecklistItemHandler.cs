using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Commands.SaveChecklistItem;

public sealed class SaveChecklistItemHandler : IRequestHandler<SaveChecklistItemCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public SaveChecklistItemHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(SaveChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.TaskToken.Decrypt<int>();
        var checklistId = request.ChecklistToken.Decrypt<int>();
        if (taskId is null || checklistId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.ChecklistNotFound);

        var itemId = 0;
        if (!string.IsNullOrEmpty(request.ItemToken))
        {
            var decrypted = request.ItemToken.Decrypt<int>();
            if (decrypted is null)
                return ApiResponse<bool>.NotFound(ErrorMessages.ChecklistNotFound);
            itemId = decrypted.Value;
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Id", itemId);
        parameters.Add("@ChecklistId", checklistId.Value);
        parameters.Add("@TaskId", taskId.Value);
        parameters.Add("@Text", request.Text.Trim());
        parameters.Add("@UserId", _userContext.UserId);

        var result = await _db.QuerySingleOrDefaultAsync<SaveChecklistResultDto>("dbo.SaveChecklistItem", parameters, cancellationToken);
        if (result is not { Result: 0 })
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
