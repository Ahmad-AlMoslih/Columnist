using Columnist.Auth.Security.CurrentUser;
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

    public SaveChecklistHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
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

        var boardId = await GetBoardIdByTaskAsync(taskId.Value, cancellationToken);
        if (boardId is not null)
            await _notifier.BoardChangedAsync(boardId.Value, cancellationToken);
        return ApiResponse<string>.Success(result.Id.Value.Encrypt());
    }

    private async Task<int?> GetBoardIdByTaskAsync(int taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        return await _db.QuerySingleOrDefaultAsync<int?>("dbo.GetBoardIdByTask", parameters, ct);
    }
}
