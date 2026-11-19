using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.MoveTask
{
    // RowVersion (Base64): when given, the move is refused if the task changed since then. The board's drag and drop leaves it null.
    [RequirePermission(Permission.TasksWrite)]
    public sealed record MoveTaskCommand(string Token, string ColumnToken, int Position, string? RowVersion = null) : IRequest<ApiResponse<bool>>, IBoardRequest
    {
        public BoardScope GetBoardScope() => new(ColumnToken: ColumnToken, TaskToken: Token);
    }
}
