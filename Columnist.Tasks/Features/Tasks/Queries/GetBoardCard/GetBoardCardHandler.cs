using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Models;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Utilities.Constants;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoardCard;

// Built like a card of GetBoard, from the same procedures (no single-task variant of them exists).
public sealed class GetBoardCardHandler : IRequestHandler<GetBoardCardQuery, ApiResponse<GetBoardTaskModel>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserPhotoService _photos;
    private readonly IBoardAccessService _boards;

    public GetBoardCardHandler(IDbExecutor db, IDmsService dms, IUserPhotoService photos, IBoardAccessService boards)
    {
        _db = db;
        _dms = dms;
        _photos = photos;
        _boards = boards;
    }

    public async Task<ApiResponse<GetBoardTaskModel>> Handle(GetBoardCardQuery request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        // The task's board was already read by AuthorizationBehavior (per-request cache).
        var boardIds = await _boards.ResolveAsync(new BoardScope(TaskToken: request.Token), cancellationToken);
        if (taskId is null || boardIds is not [var boardId])
            return ApiResponse<GetBoardTaskModel>.NotFound(ErrorMessages.TaskNotFound);

        var boardParameters = new DynamicParameters();
        boardParameters.Add("@BoardId", boardId);
        var task = (await _db.QueryAsync<TaskItemDto>("dbo.GetAllTasksWithDetails", boardParameters, cancellationToken))
            .FirstOrDefault(t => t.Id == taskId.Value);
        if (task is null)
            return ApiResponse<GetBoardTaskModel>.NotFound(ErrorMessages.TaskNotFound);

        var assigneeParameters = new DynamicParameters();
        assigneeParameters.Add("@TaskId", taskId.Value);
        assigneeParameters.Add("@BoardId", null);
        var assigneeRows = await _db.QueryAsync<TaskAssigneeDto>("dbo.GetTaskAssignees", assigneeParameters, cancellationToken);
        var photos = await _photos.GetUrlsAsync(cancellationToken);
        var assignees = assigneeRows
            .Select(a => TaskAssigneeModel.From(a) with { PhotoUrl = photos.GetValueOrDefault(a.UserId) })
            .ToList();

        var checklists = (await _db.QueryAsync<BoardChecklistCountDto>("dbo.GetBoardChecklistCounts", boardParameters, cancellationToken))
            .FirstOrDefault(c => c.TaskId == taskId.Value);

        var canWrite = await _boards.HasPermissionAsync(boardId, Permission.TasksWrite, cancellationToken);
        var canDelete = await _boards.HasPermissionAsync(boardId, Permission.TasksDelete, cancellationToken);

        return ApiResponse<GetBoardTaskModel>.Success(GetBoardTaskModel.From(task, assignees, checklists,
            _dms.GetUrl(DmsFolder.Task, task.FirstImageName), canWrite, canDelete));
    }
}
