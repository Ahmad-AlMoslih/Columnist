using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskComments.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskComments;

public sealed class GetTaskCommentsHandler : IRequestHandler<GetTaskCommentsQuery, ApiResponse<GetTaskCommentsModel>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetTaskCommentsHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<GetTaskCommentsModel>> Handle(GetTaskCommentsQuery request, CancellationToken cancellationToken)
    {
        var taskId = request.TaskToken.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<GetTaskCommentsModel>.NotFound(ErrorMessages.TaskNotFound);

        var editId = string.IsNullOrEmpty(request.EditCommentToken) ? null : request.EditCommentToken.Decrypt<int>();

        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId.Value);
        var rows = await _db.QueryAsync<TaskCommentDto>("dbo.GetTaskComments", parameters, cancellationToken);

        var userId = _userContext.UserId;
        var photos = await _photos.GetUrlsAsync(cancellationToken);
        IReadOnlyList<TaskCommentModel> comments = rows
            .Select(c => new TaskCommentModel
            {
                Token = c.Id.Encrypt(),
                AuthorName = c.AuthorName,
                AuthorInitials = Initials(c.AuthorName),
                PhotoUrl = photos.GetValueOrDefault(c.UserId),
                Body = c.Body,
                CreatedAt = c.CreatedAt,
                IsEdited = c.UpdatedAt is not null,
                IsMine = c.UserId == userId,
                IsEditing = c.UserId == userId && c.Id == editId
            })
            .ToList();

        return ApiResponse<GetTaskCommentsModel>.Success(new GetTaskCommentsModel
        {
            TaskToken = request.TaskToken,
            Comments = comments
        });
    }

    private static string Initials(string fullName) =>
        string.Concat(fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0])));
}
