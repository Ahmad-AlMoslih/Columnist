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

    public GetTaskChecklistsHandler(IDbExecutor db) => _db = db;

    public async Task<ApiResponse<GetTaskChecklistsModel>> Handle(GetTaskChecklistsQuery request, CancellationToken cancellationToken)
    {
        var taskId = request.TaskToken.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<GetTaskChecklistsModel>.NotFound(ErrorMessages.TaskNotFound);

        var addingId = string.IsNullOrEmpty(request.AddItemChecklistToken) ? null : request.AddItemChecklistToken.Decrypt<int>();

        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId.Value);
        var rows = await _db.QueryAsync<TaskChecklistRowDto>("dbo.GetTaskChecklists", parameters, cancellationToken);

        return ApiResponse<GetTaskChecklistsModel>.Success(new GetTaskChecklistsModel
        {
            TaskToken = request.TaskToken,
            Checklists = ChecklistModel.FromRows(rows, addingId)
        });
    }
}
