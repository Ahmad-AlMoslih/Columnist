using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Commands.DeleteChecklist;

[RequirePermission(Permission.TasksWrite)]
public sealed record DeleteChecklistCommand(string TaskToken, string ChecklistToken) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: TaskToken);
}
