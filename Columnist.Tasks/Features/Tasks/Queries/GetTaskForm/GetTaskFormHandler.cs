using Dapper;
using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
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
    private readonly IBoardAccessService _boards;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetTaskFormHandler(IDbExecutor db, IDmsService dms, IBoardAccessService boards, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _dms = dms;
        _boards = boards;
        _userContext = userContext;
        _photos = photos;
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
            var assignees = await GetTaskAssigneesAsync(task.Id, cancellationToken);

            form = new TaskFormDto
            {
                Token = task.Id.Encrypt(),
                Title = task.Title,
                Description = task.Description,
                ColumnToken = task.ColumnId.Encrypt(),
                AssigneeTokens = assignees.Select(a => a.UserId.Encrypt()).ToList(),
                DueDate = task.DueDate,
                ReturnBoardToken = request.ReturnBoardToken,
                RowVersion = task.RowVersion is null ? null : Convert.ToBase64String(task.RowVersion),
                ImageUrls = imageNames.Select(name => _dms.GetUrl(DmsFolder.Task, name)!).ToList()
            };
        }

        int? selectedColumnId = form.ColumnToken.Decrypt<int>();
        var selectedAssigneeIds = (form.AssigneeTokens ?? [])
            .Select(token => token.Decrypt<int>())
            .OfType<int>()
            .ToHashSet();

        var scope = !string.IsNullOrEmpty(request.Token) ? new BoardScope(TaskToken: request.Token)
                  : !string.IsNullOrEmpty(request.ColumnToken) ? new BoardScope(ColumnToken: request.ColumnToken)
                  : new BoardScope(BoardToken: request.ReturnBoardToken);
        var boardIds = await _boards.ResolveAsync(scope, cancellationToken);
        int? boardId = boardIds is { Count: > 0 } ? boardIds[0] : null;
        int? dropdownTaskId = string.IsNullOrEmpty(request.Token) ? null : request.Token.Decrypt<int>();

        var columns = await GetColumnOptionsAsync(boardId, cancellationToken);
        var users = await GetUserOptionsAsync(boardId, dropdownTaskId, cancellationToken);
        var photos = await _photos.GetUrlsAsync(cancellationToken);
        form.Columns = ToTokenOptions(columns, selectedColumnId);
        form.Users = users
            .Select(u => new AssigneeOptionDto { Token = u.Id.Encrypt(), Name = u.Name, PhotoUrl = photos.GetValueOrDefault(u.Id), Selected = selectedAssigneeIds.Contains(u.Id), HasAccess = u.HasAccess })
            .ToList();

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

    private async Task<IReadOnlyList<TaskAssigneeDto>> GetTaskAssigneesAsync(int taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        return await _db.QueryAsync<TaskAssigneeDto>("dbo.GetTaskAssignees", parameters, ct);
    }

    private IReadOnlyList<TokenOptionDto> ToTokenOptions(IReadOnlyList<OptionDto> options, int? selectedId)
    {
        return options
            .Select(o => new TokenOptionDto { Token = o.Id.Encrypt(), Name = o.Name, Selected = o.Id == selectedId })
            .ToList();
    }

    private async Task<IReadOnlyList<OptionDto>> GetColumnOptionsAsync(int? boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        parameters.Add("@UserId", _userContext.UserId);
        return await _db.QueryAsync<OptionDto>("dbo.GetColumnsForDropdown", parameters, ct);
    }

    private async Task<IReadOnlyList<UserOptionDto>> GetUserOptionsAsync(int? boardId, int? taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        parameters.Add("@TaskId", taskId);
        parameters.Add("@UserId", _userContext.UserId);
        return await _db.QueryAsync<UserOptionDto>("dbo.GetUsersForDropdown", parameters, ct);
    }
}
