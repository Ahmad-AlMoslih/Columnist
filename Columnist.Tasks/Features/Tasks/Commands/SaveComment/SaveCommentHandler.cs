using Columnist.Auth.Security.CurrentUser;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveComment;

public sealed class SaveCommentHandler : IRequestHandler<SaveCommentCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public SaveCommentHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(SaveCommentCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.TaskToken.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var commentId = 0;
        if (!string.IsNullOrEmpty(request.CommentToken))
        {
            var decrypted = request.CommentToken.Decrypt<int>();
            if (decrypted is null)
                return ApiResponse<bool>.NotFound(ErrorMessages.CommentNotFound);
            commentId = decrypted.Value;
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Id", commentId);
        parameters.Add("@TaskId", taskId.Value);
        parameters.Add("@Body", request.Body.Trim());
        parameters.Add("@UserId", _userContext.UserId);

        var saved = await _db.ExecuteAsync("dbo.SaveTaskComment", parameters, cancellationToken);
        if (!saved)
            return ApiResponse<bool>.NotFound(ErrorMessages.CommentNotFound);

        var boardId = await GetBoardIdByTaskAsync(taskId.Value, cancellationToken);
        if (boardId is not null)
            await _notifier.CommentsChangedAsync(boardId.Value, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }

    private async Task<int?> GetBoardIdByTaskAsync(int taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        return await _db.QuerySingleOrDefaultAsync<int?>("dbo.GetBoardIdByTask", parameters, ct);
    }
}
