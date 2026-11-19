using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks.Models;
using Columnist.Utilities.Constants;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks;

public sealed class GetDeletedTasksHandler : IRequestHandler<GetDeletedTasksQuery, ApiResponse<GetDeletedTasksModel>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IBoardAccessService _boards;

    public GetDeletedTasksHandler(IDbExecutor db, IDmsService dms, IBoardAccessService boards)
    {
        _db = db;
        _dms = dms;
        _boards = boards;
    }

    public async Task<ApiResponse<GetDeletedTasksModel>> Handle(GetDeletedTasksQuery request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken?.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<GetDeletedTasksModel>.NotFound(ErrorMessages.BoardNotFound);

        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId.Value);

        var rows = await _db.QueryAsync<DeletedTaskDto>("dbo.GetDeletedTasks", parameters, cancellationToken);
        var search = request.Search?.Trim();
        if (search is { Length: >= SearchRules.MinLength })
            rows = rows.Where(r => r.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        var images = (await _db.QueryAsync<DeletedTaskImageDto>("dbo.GetDeletedTaskImages", parameters, cancellationToken))
            .ToLookup(i => i.TaskId);

        IReadOnlyList<DeletedTaskModel> tasks = rows
            .Select(t => new DeletedTaskModel
            {
                Token = t.Id.Encrypt(),
                Key = t.Id.ToStableKey("task"),
                Title = t.Title,
                BoardName = t.BoardName,
                ColumnName = t.ColumnName,
                DeletedAt = t.DeletedAt,
                DeletedByName = t.DeletedByName,
                ImageUrls = images[t.Id].Select(i => _dms.GetUrl(DmsFolder.Task, i.FileName)!).ToList()
            })
            .ToList();

        return ApiResponse<GetDeletedTasksModel>.Success(new GetDeletedTasksModel
        {
            Tasks = tasks,
            Search = request.Search,
            BoardToken = request.BoardToken,
            CanSeeDeleted = await _boards.HasPermissionAsync(boardId.Value, Permission.DeletedTasksRead, cancellationToken),
            CanSeeLogs = await _boards.HasPermissionAsync(boardId.Value, Permission.LogsRead, cancellationToken),
            CanManage = await _boards.IsOwnerAsync(boardId.Value, cancellationToken),
            CanRestore = await _boards.HasPermissionAsync(boardId.Value, Permission.DeletedTasksWrite, cancellationToken)
        });
    }
}
