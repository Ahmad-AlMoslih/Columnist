using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.SetTaskAssignees;

// AssigneeTokens: the full list after the add or remove (encrypted user ids).
[RequirePermission(Permission.TasksWrite)]
public sealed record SetTaskAssigneesCommand(string Token, IReadOnlyList<string> AssigneeTokens, string? RowVersion) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
