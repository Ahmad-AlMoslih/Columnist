using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Commands.RestoreTask;

[RequirePermission(Permission.DeletedTasksWrite)]
public sealed record RestoreTaskCommand(string Token) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
