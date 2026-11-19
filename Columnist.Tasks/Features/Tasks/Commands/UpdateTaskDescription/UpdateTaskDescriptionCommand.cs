using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.UpdateTaskDescription;

// Description: null or empty clears it.
[RequirePermission(Permission.TasksWrite)]
public sealed record UpdateTaskDescriptionCommand(string Token, string? Description, string? RowVersion) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
