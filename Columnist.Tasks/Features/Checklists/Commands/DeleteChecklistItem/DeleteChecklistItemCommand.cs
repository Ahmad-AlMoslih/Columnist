using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Commands.DeleteChecklistItem;

[RequirePermission(Permission.TasksWrite)]
public sealed record DeleteChecklistItemCommand(string TaskToken, string ChecklistToken, string ItemToken) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: TaskToken);
}
