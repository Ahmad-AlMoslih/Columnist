using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveComment;

// CommentToken empty = new comment; otherwise edit (author only, checked in the procedure).
[RequireBoardAccess]
public sealed record SaveCommentCommand(string TaskToken, string? CommentToken, string Body) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public const int BodyMaxLength = 1000;   // = TaskComments.Body NVARCHAR(1000)

    public BoardScope GetBoardScope() => new(TaskToken: TaskToken);
}
