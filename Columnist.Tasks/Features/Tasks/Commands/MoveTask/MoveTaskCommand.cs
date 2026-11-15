using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.MoveTask
{
    [RequirePermission(Permission.TasksWrite)]
    public sealed record MoveTaskCommand(string Token, string ColumnToken, int Position) : IRequest<ApiResponse<bool>>, IBoardRequest
    {
        public BoardScope GetBoardScope() => new(ColumnToken: ColumnToken, TaskToken: Token);
    }
}
