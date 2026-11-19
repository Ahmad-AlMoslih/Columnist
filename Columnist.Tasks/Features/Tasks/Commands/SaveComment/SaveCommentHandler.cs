using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Boards;
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
    private readonly IBoardAccessService _boards;

    public SaveCommentHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
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

        // The task's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.CommentsChangedAsync(boardId, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}
