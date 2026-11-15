using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.DeleteTask
{
    [RequirePermission(Permission.TasksDelete)]
    public sealed record DeleteTaskCommand(string Token) : IRequest<ApiResponse<bool>>, IBoardRequest
    {
        public BoardScope GetBoardScope() => new(TaskToken: Token);
    }
}