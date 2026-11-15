using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskView.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskView;

[RequireBoardAccess]
public sealed record GetTaskViewQuery(string Token) : IRequest<ApiResponse<GetTaskViewModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
