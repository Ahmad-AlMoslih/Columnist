using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Commands.SaveChecklist;

// ChecklistToken empty = new checklist; otherwise rename. Returns the checklist's token.
[RequirePermission(Permission.TasksWrite)]
public sealed record SaveChecklistCommand(string TaskToken, string? ChecklistToken, string Title) : IRequest<ApiResponse<string>>, IBoardRequest
{
    public const int TitleMaxLength = 100;

    public BoardScope GetBoardScope() => new(TaskToken: TaskToken);
}
