using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.UpdateTaskTitle;

[RequirePermission(Permission.TasksWrite)]
public sealed record UpdateTaskTitleCommand(string Token, string Title, string? RowVersion) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
