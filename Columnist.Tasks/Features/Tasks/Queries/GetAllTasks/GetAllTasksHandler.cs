using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Models;
using Dapper;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetAllTasks;

public sealed class GetAllTasksHandler : IRequestHandler<GetAllTasksQuery, ApiResponse<IReadOnlyList<GetAllTasksModel>>>
{
    private readonly IDbExecutor _dbExecutor;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;
    public GetAllTasksHandler(IDbExecutor db, IDmsService dms, IUserContext userContext, IUserPhotoService photos)
    {
        _dbExecutor = db;
        _dms = dms;
        _userContext = userContext;
        _photos = photos;
    }
    public async Task<ApiResponse<IReadOnlyList<GetAllTasksModel>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await GetAllTasksAsync(cancellationToken);

        // dbo.GetTaskAssignees works per task or per board: one call per board in the list.
        var rows = new List<TaskAssigneeDto>();
        foreach (var boardId in tasks.Select(t => t.BoardId).Distinct())
            rows.AddRange(await GetTaskAssigneesAsync(null, boardId, cancellationToken));
        var photos = await _photos.GetUrlsAsync(cancellationToken);
        var assignees = rows.ToLookup(a => a.TaskId, a => TaskAssigneeModel.From(a) with { PhotoUrl = photos.GetValueOrDefault(a.UserId) });

        return ApiResponse<IReadOnlyList<GetAllTasksModel>>.Success(tasks.Select(t => ToModel(t, assignees[t.Id].ToList())).ToList());
    }

    private GetAllTasksModel ToModel(TaskItemDto task, IReadOnlyList<TaskAssigneeModel> assignees)
    {
        return new GetAllTasksModel
        {
            Token = task.Id.Encrypt(),
            Title = task.Title,
            Description = task.Description,
            BoardToken = task.BoardId.Encrypt(),
            BoardName = task.BoardName,
            ColumnToken = task.ColumnId.Encrypt(),
            ColumnName = task.ColumnName,
            Assignees = assignees,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            StatusColor = task.StatusColor,
            FirstImageName = task.FirstImageName,
            ImageCount = task.ImageCount,
            FirstImageUrl = _dms.GetUrl(DmsFolder.Task, task.FirstImageName)
        };
    }

    private async Task<IReadOnlyList<TaskItemDto>> GetAllTasksAsync(CancellationToken ct)
    {
        // @BoardId NULL + @UserId: only tasks on boards the caller can open.
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", null);
        parameters.Add("@UserId", _userContext.UserId);
        return await _dbExecutor.QueryAsync<TaskItemDto>("dbo.GetAllTasksWithDetails", parameters, ct);
    }

    private async Task<IReadOnlyList<TaskAssigneeDto>> GetTaskAssigneesAsync(int? taskId, int? boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        parameters.Add("@BoardId", boardId);
        return await _dbExecutor.QueryAsync<TaskAssigneeDto>("dbo.GetTaskAssignees", parameters, ct);
    }
}
