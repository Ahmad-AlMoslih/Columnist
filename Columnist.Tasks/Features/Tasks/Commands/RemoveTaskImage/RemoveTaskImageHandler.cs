using Dapper;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Boards;
using Columnist.Services.Tokens.Extensions;
using Columnist.Services.Realtime;
using MediatR;
using System.Data;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands.RemoveTaskImage;

public sealed class RemoveTaskImageHandler : IRequestHandler<RemoveTaskImageCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;
    private readonly IBoardAccessService _boards;

    public RemoveTaskImageHandler(IDbExecutor db, IDmsService dms, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _dms = dms;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<bool>> Handle(RemoveTaskImageCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);
        var imageId = request.ImageToken.Decrypt<int>();
        if (imageId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.ImageNotFound);
        if (!TaskFieldSave.TryParseRowVersion(request.RowVersion, out var rowVersion))
            return ApiResponse<bool>.Conflict(ErrorMessages.TaskChangedByOther);

        // The procedure checks the image belongs to this task.
        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        parameters.Add("@ImageId", imageId.Value);
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@RowVersion", rowVersion, DbType.Binary, size: 8);
        parameters.Add("@FileName", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);
        if (!await _db.ExecuteAsync("dbo.RemoveTaskImage", parameters, cancellationToken))
            return TaskFieldSave.Failure(parameters.Get<int>("@ReturnValue"));

        // After the commit, so a failed save never loses the file.
        _dms.Delete(DmsFolder.Task, parameters.Get<string?>("@FileName"));

        // The task's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}
