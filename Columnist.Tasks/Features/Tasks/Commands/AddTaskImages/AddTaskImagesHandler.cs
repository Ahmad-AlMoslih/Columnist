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

namespace Columnist.Tasks.Features.Tasks.Commands.AddTaskImages;

public sealed class AddTaskImagesHandler : IRequestHandler<AddTaskImagesCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;
    private readonly IBoardAccessService _boards;

    public AddTaskImagesHandler(IDbExecutor db, IDmsService dms, IUserContext userContext, IBoardNotifier notifier, IBoardAccessService boards)
    {
        _db = db;
        _dms = dms;
        _userContext = userContext;
        _notifier = notifier;
        _boards = boards;
    }

    public async Task<ApiResponse<bool>> Handle(AddTaskImagesCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);
        if (!TaskFieldSave.TryParseRowVersion(request.RowVersion, out var rowVersion))
            return ApiResponse<bool>.Conflict(ErrorMessages.TaskChangedByOther);

        // New files are deleted automatically on any early return or exception, unless committed.
        // The batch checks the count, size and type again (DmsOptions).
        using var uploads = _dms.BeginUpload(DmsFolder.Task);
        foreach (var image in request.Images)
        {
            var upload = await uploads.AddAsync(image, cancellationToken);
            if (!upload.Success)
                return ApiResponse<bool>.BadRequest(upload.Error!, nameof(request.Images));
        }

        // dbo.ImageNameList: FileName.
        var images = new DataTable();
        images.Columns.Add("FileName", typeof(string));
        foreach (var fileName in uploads.FileNames)
            images.Rows.Add(fileName);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        parameters.Add("@Images", images.AsTableValuedParameter("dbo.ImageNameList"));
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@RowVersion", rowVersion, DbType.Binary, size: 8);
        if (!await _db.ExecuteAsync("dbo.AddTaskImages", parameters, cancellationToken))
            return TaskFieldSave.Failure(parameters.Get<int>("@ReturnValue"));

        uploads.Commit();

        // The task's board was already read by AuthorizationBehavior (per-request cache): no second lookup.
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is [var boardId])
            await _notifier.BoardChangedAsync(boardId, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}
