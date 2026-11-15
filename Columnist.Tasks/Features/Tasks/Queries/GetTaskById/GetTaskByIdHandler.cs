using Dapper;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Models;
using MediatR;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, ApiResponse<GetTaskByIdModel>>
{
    private readonly IDbExecutor _db;

    public GetTaskByIdHandler(IDbExecutor db)
    {
        _db = db;
    }

    public async Task<ApiResponse<GetTaskByIdModel>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<GetTaskByIdModel>.NotFound(ErrorMessages.TaskNotFound);

        var task = await GetTaskByIdAsync(taskId.Value, cancellationToken);
        if (task is null)
            return ApiResponse<GetTaskByIdModel>.NotFound(ErrorMessages.TaskNotFound);

        var assignees = await GetTaskAssigneesAsync(taskId.Value, null, cancellationToken);

        return ApiResponse<GetTaskByIdModel>.Success(ToModel(task, assignees));
    }

    private GetTaskByIdModel ToModel(TaskDetailsDto task, IReadOnlyList<TaskAssigneeDto> assignees)
    {
        return new GetTaskByIdModel
        {
            Token = task.Id.Encrypt(),
            Title = task.Title,
            Description = task.Description,
            ColumnToken = task.ColumnId.Encrypt(),
            Assignees = assignees.Select(TaskAssigneeModel.From).ToList(),
            DueDate = task.DueDate
        };
    }

    private async Task<TaskDetailsDto?> GetTaskByIdAsync(int id, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await _db.QuerySingleOrDefaultAsync<TaskDetailsDto>("dbo.GetTaskById", parameters, ct);
    }

    private async Task<IReadOnlyList<TaskAssigneeDto>> GetTaskAssigneesAsync(int? taskId, int? boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        parameters.Add("@BoardId", boardId);
        return await _db.QueryAsync<TaskAssigneeDto>("dbo.GetTaskAssignees", parameters, ct);
    }
}
