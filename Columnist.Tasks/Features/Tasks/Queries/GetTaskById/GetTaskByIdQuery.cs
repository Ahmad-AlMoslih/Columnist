using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskById;

[RequireBoardAccess]
public sealed record GetTaskByIdQuery(string Token) : IRequest<ApiResponse<GetTaskByIdModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
