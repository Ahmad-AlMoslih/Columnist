using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetAllTasks;

public sealed class GetAllTasksHandler : IRequestHandler<GetAllTasksQuery, ApiResponse<IReadOnlyList<GetAllTasksModel>>>
{
    private readonly IDbExecutor _dbExecutor;
    private readonly IDmsService _dms;
    public GetAllTasksHandler(IDbExecutor db, IDmsService dms)
    {
        _dbExecutor = db;
        _dms = dms;
    }
    public async Task<ApiResponse<IReadOnlyList<GetAllTasksModel>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await GetAllTasksAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<GetAllTasksModel>>.Success(tasks.Select(ToModel).ToList());
    }

    private GetAllTasksModel ToModel(TaskItemDto task)
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
            AssigneeName = task.AssigneeName,
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
        return await _dbExecutor.QueryAsync<TaskItemDto>("dbo.GetAllTasksWithDetails", ct: ct);
    }
}
