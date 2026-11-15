using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskComments.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskComments;

// EditCommentToken = the comment to show as an edit box (only if it's the current user's).
[RequireBoardAccess]
public sealed record GetTaskCommentsQuery(string TaskToken, string? EditCommentToken = null) : IRequest<ApiResponse<GetTaskCommentsModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: TaskToken);
}
