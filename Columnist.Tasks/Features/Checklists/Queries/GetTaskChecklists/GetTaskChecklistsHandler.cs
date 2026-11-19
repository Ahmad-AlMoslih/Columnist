using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Checklists.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Queries.GetTaskChecklists;

public sealed class GetTaskChecklistsHandler : IRequestHandler<GetTaskChecklistsQuery, ApiResponse<GetTaskChecklistsModel>>
{
    private readonly IDbExecutor _db;
    private readonly IBoardAccessService _boards;

    public GetTaskChecklistsHandler(IDbExecutor db, IBoardAccessService boards)
    {
        _db = db;
        _boards = boards;
    }

    public async Task<ApiResponse<GetTaskChecklistsModel>> Handle(GetTaskChecklistsQuery request, CancellationToken cancellationToken)
    {
        var taskId = request.TaskToken.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<GetTaskChecklistsModel>.NotFound(ErrorMessages.TaskNotFound);

        var addingId = string.IsNullOrEmpty(request.AddItemChecklistToken) ? null : request.AddItemChecklistToken.Decrypt<int>();

        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId.Value);
        var rows = await _db.QueryAsync<TaskChecklistRowDto>("dbo.GetTaskChecklists", parameters, cancellationToken);

        // The task's board, resolved once (cached since the pipeline's check).
        var boardIds = await _boards.ResolveAsync(new BoardScope(TaskToken: request.TaskToken), cancellationToken);
        var canWrite = boardIds is [var boardId] && await _boards.HasPermissionAsync(boardId, Permission.TasksWrite, cancellationToken);

        return ApiResponse<GetTaskChecklistsModel>.Success(new GetTaskChecklistsModel
        {
            TaskToken = request.TaskToken,
            TaskKey = taskId.Value.ToStableKey("task"),
            Checklists = ChecklistModel.FromRows(rows, addingId),
            CanWrite = canWrite
        });
    }
}
