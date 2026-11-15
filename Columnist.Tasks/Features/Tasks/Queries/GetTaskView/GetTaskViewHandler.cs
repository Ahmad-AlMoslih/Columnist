using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Checklists.Models;
using Columnist.Tasks.Features.Tasks.Models;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskView.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskView;

public sealed class GetTaskViewHandler : IRequestHandler<GetTaskViewQuery, ApiResponse<GetTaskViewModel>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserPhotoService _photos;

    public GetTaskViewHandler(IDbExecutor db, IDmsService dms, IUserPhotoService photos)
    {
        _db = db;
        _dms = dms;
        _photos = photos;
    }

    public async Task<ApiResponse<GetTaskViewModel>> Handle(GetTaskViewQuery request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<GetTaskViewModel>.NotFound(ErrorMessages.TaskNotFound);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        var task = await _db.QuerySingleOrDefaultAsync<TaskViewDto>("dbo.GetTaskDetails", parameters, cancellationToken);
        if (task is null)
            return ApiResponse<GetTaskViewModel>.NotFound(ErrorMessages.TaskNotFound);

        var imageParameters = new DynamicParameters();
        imageParameters.Add("@TaskId", taskId.Value);
        var images = await _db.QueryAsync<string>("dbo.GetImagesByTaskId", imageParameters, cancellationToken);

        var assignees = await GetTaskAssigneesAsync(taskId.Value, null, cancellationToken);

        var checklistParameters = new DynamicParameters();
        checklistParameters.Add("@TaskId", taskId.Value);
        var checklists = await _db.QueryAsync<TaskChecklistRowDto>("dbo.GetTaskChecklists", checklistParameters, cancellationToken);

        var historyParameters = new DynamicParameters();
        historyParameters.Add("@TaskId", taskId.Value);
        var history = await _db.QueryAsync<AuditLogRowDto>("dbo.GetTaskHistory", historyParameters, cancellationToken);

        var photos = await _photos.GetUrlsAsync(cancellationToken);

        return ApiResponse<GetTaskViewModel>.Success(new GetTaskViewModel
        {
            Token = request.Token,
            Title = task.Title,
            Description = task.Description,
            BoardName = task.BoardName,
            ColumnName = task.ColumnName,
            StatusColor = task.StatusColor,
            StatusName = task.StatusName,
            Assignees = assignees.Select(a => TaskAssigneeModel.From(a) with { PhotoUrl = photos.GetValueOrDefault(a.UserId) }).ToList(),
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            CreatedByName = task.CreatedByName,
            UpdatedAt = task.UpdatedAt,
            UpdatedByName = task.UpdatedByName,
            ImageUrls = images.Select(name => _dms.GetUrl(DmsFolder.Task, name)!).ToList(),
            Checklists = ChecklistModel.FromRows(checklists),
            History = history
                .OrderByDescending(h => h.CreatedAt)
                .ThenByDescending(h => h.Id)
                .Select(h => new AuditEntryModel
                {
                    ActorName = h.ActorName,
                    ActorInitials = GetInitials(h.ActorName),
                    PhotoUrl = photos.GetValueOrDefault(h.ActorId),
                    Action = h.Action,
                    Category = h.Category,
                    TargetName = h.TargetName,
                    Field = h.Field,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    CreatedAt = h.CreatedAt
                })
                .ToList()
        });
    }

    // First letters of the first two words of the name, uppercase.
    private static string GetInitials(string fullName) =>
        string.Concat(fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0])));

    private async Task<IReadOnlyList<TaskAssigneeDto>> GetTaskAssigneesAsync(int? taskId, int? boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        parameters.Add("@BoardId", boardId);
        return await _db.QueryAsync<TaskAssigneeDto>("dbo.GetTaskAssignees", parameters, ct);
    }
}
