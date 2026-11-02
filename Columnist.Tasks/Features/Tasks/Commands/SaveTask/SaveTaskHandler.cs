using Dapper;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Services.Tokens.Extensions;
using MediatR;
using System.Data;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveTask;

public sealed class SaveTaskHandler : IRequestHandler<SaveTaskCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;

    public SaveTaskHandler(IDbExecutor db, IDmsService dms, IUserContext userContext)
    {
        _db = db;
        _dms = dms;
        _userContext = userContext;
    }

    public async Task<ApiResponse<bool>> Handle(SaveTaskCommand request, CancellationToken ct)
    {
        var taskId = 0;
        if (!request.Task.IsNew)
        {
            var decryptedId = request.Task.Token.Decrypt<int>();
            if (decryptedId is null)
                return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);
            taskId = decryptedId.Value;
        }

        DateTime? originalDueDate = null;
        List<string> oldImages = [];

        if (!request.Task.IsNew)
        {
            var existing = await GetTaskByIdAsync(taskId, ct);
            if (existing is null)
                return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

            originalDueDate = existing.DueDate;

            oldImages = await GetTaskImagesAsync(taskId, ct);
        }

        bool dueDateChanged = request.Task.DueDate?.Date != originalDueDate?.Date;
        if (dueDateChanged && request.Task.DueDate is not null && request.Task.DueDate.Value.Date < DateTime.Today)
            return ApiResponse<bool>.BadRequest(ErrorMessages.DueDateInPast, nameof(TaskFormDto.DueDate));

        var columnId = request.Task.ColumnToken.Decrypt<int>();
        if (columnId is null)
            return ApiResponse<bool>.BadRequest(ErrorMessages.ChooseStatus, nameof(TaskFormDto.ColumnToken));

        int? assigneeId = null;
        if (!string.IsNullOrEmpty(request.Task.AssigneeToken))
        {
            assigneeId = request.Task.AssigneeToken.Decrypt<int>();
            if (assigneeId is null)
                return ApiResponse<bool>.BadRequest(ErrorMessages.InvalidAssignee, nameof(TaskFormDto.AssigneeToken));
        }

        // New files are deleted automatically on any early return or exception, unless committed.
        using var uploads = _dms.BeginUpload(DmsFolder.Task);
        foreach (var image in request.Task.Images ?? [])
        {
            var upload = await uploads.AddAsync(image, ct);
            if (!upload.Success)
                return ApiResponse<bool>.BadRequest(upload.Error!, nameof(TaskFormDto.Images));
        }

        var saved = await SaveTaskAsync(taskId, columnId.Value, assigneeId, request, uploads.FileNames, ct);
        if (!saved)
            return ApiResponse<bool>.BadRequest(ErrorMessages.SaveFailed);

        uploads.Commit();

        if (request.Task.RemoveImages)
            _dms.Delete(DmsFolder.Task, oldImages);

        return ApiResponse<bool>.Success(true);
    }

    private async Task<TaskDetailsDto?> GetTaskByIdAsync(int id, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await _db.QuerySingleOrDefaultAsync<TaskDetailsDto>("dbo.GetTaskById", parameters, ct);
    }

    private async Task<List<string>> GetTaskImagesAsync(int taskId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        var images = await _db.QueryAsync<string>("dbo.GetImagesByTaskId", parameters, ct);
        return images.ToList();
    }

    private async Task<bool> SaveTaskAsync(int taskId, int columnId, int? assigneeId, SaveTaskCommand request, IReadOnlyList<string> imageNames, CancellationToken ct)
    {
        var images = new DataTable();
        images.Columns.Add("FileName", typeof(string));
        foreach (var fileName in imageNames)
            images.Rows.Add(fileName);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId);
        parameters.Add("@ColumnId", columnId);
        parameters.Add("@Title", request.Task.Title);
        parameters.Add("@Description", request.Task.Description);
        parameters.Add("@AssigneeId", assigneeId);
        parameters.Add("@DueDate", request.Task.DueDate);
        parameters.Add("@RemoveImages", request.Task.RemoveImages);
        parameters.Add("@Images", images.AsTableValuedParameter("dbo.ImageNameList"));
        parameters.Add("@UserId", _userContext.UserId);

        return await _db.ExecuteAsync("dbo.SaveTaskWithImages", parameters, ct);
    }
}
