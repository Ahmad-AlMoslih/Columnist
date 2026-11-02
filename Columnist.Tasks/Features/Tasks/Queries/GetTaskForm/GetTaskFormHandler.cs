using Dapper;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using MediatR;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskForm;

public sealed class GetTaskFormHandler : IRequestHandler<GetTaskFormQuery, ApiResponse<TaskFormDto>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;

    public GetTaskFormHandler(IDbExecutor db, IDmsService dms)
    {
        _db = db;
        _dms = dms;
    }

    public async Task<ApiResponse<TaskFormDto>> Handle(GetTaskFormQuery request, CancellationToken cancellationToken)
    {
        TaskFormDto form;

        if (request.Posted is not null)
        {
            form = request.Posted;
        }
        else if (string.IsNullOrEmpty(request.Token))
        {
            var columnId = request.ColumnToken.Decrypt<int>();
            form = new TaskFormDto
            {
                ColumnToken = columnId is null ? null : columnId.Value.Encrypt(),
                ReturnBoardToken = request.ReturnBoardToken
            };
        }
        else
        {
            var taskId = request.Token.Decrypt<int>();
            if (taskId is null)
                return ApiResponse<TaskFormDto>.NotFound(ErrorMessages.TaskNotFound);

            var task = await GetTaskByIdAsync(taskId.Value, cancellationToken);
            if (task is null)
                return ApiResponse<TaskFormDto>.NotFound(ErrorMessages.TaskNotFound);

            var imageNames = await GetTaskImagesAsync(task.Id, cancellationToken);

            form = new TaskFormDto
            {
                Token = task.Id.Encrypt(),
                Title = task.Title,
                Description = task.Description,
                ColumnToken = task.ColumnId.Encrypt(),
                AssigneeToken = task.AssigneeId is null ? null : task.AssigneeId.Value.Encrypt(),
                DueDate = task.DueDate,
                ReturnBoardToken = request.ReturnBoardToken,
                ImageUrls = imageNames.Select(name => _dms.GetUrl(DmsFolder.Task, name)!).ToList()
            };
        }

        int? selectedColumnId = form.ColumnToken.Decrypt<int>();
        int? selectedAssigneeId = form.AssigneeToken.Decrypt<int>();

        var columns = await GetColumnOptionsAsync(cancellationToken);
        var users = await GetUserOptionsAsync(cancellationToken);
        form.Columns = ToTokenOptions(columns, selectedColumnId);
        form.Users = ToTokenOptions(users, selectedAssigneeId);

        return ApiResponse<TaskFormDto>.Success(form);
    }

    private async Task<TaskDetailsDto?> GetTaskByIdAsync(int id, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await _db.QuerySingleOrDefaultAsync<TaskDetailsDto>("dbo.GetTaskById", parameters, ct);
    }

    private async Task<IReadOnlyList<string>> GetTaskImagesAsync(int taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        return await _db.QueryAsync<string>("dbo.GetImagesByTaskId", parameters, ct);
    }

    private IReadOnlyList<TokenOptionDto> ToTokenOptions(IReadOnlyList<OptionDto> options, int? selectedId)
    {
        return options
            .Select(o => new TokenOptionDto { Token = o.Id.Encrypt(), Name = o.Name, Selected = o.Id == selectedId })
            .ToList();
    }

    private async Task<IReadOnlyList<OptionDto>> GetColumnOptionsAsync(CancellationToken ct)
    {
        return await _db.QueryAsync<OptionDto>("dbo.GetColumnsForDropdown", ct: ct);
    }

    private async Task<IReadOnlyList<OptionDto>> GetUserOptionsAsync(CancellationToken ct)
    {
        return await _db.QueryAsync<OptionDto>("dbo.GetUsersForDropdown", ct: ct);
    }
}
