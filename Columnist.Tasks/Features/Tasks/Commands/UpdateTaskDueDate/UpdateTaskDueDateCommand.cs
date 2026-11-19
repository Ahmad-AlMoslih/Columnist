using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.UpdateTaskDueDate;

// DueDate: null clears it.
[RequirePermission(Permission.TasksWrite)]
public sealed record UpdateTaskDueDateCommand(string Token, DateTime? DueDate, string? RowVersion) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
