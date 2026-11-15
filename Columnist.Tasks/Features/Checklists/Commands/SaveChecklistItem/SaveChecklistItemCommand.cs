using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Commands.SaveChecklistItem;

// ItemToken empty = new item; otherwise edit its text.
[RequirePermission(Permission.TasksWrite)]
public sealed record SaveChecklistItemCommand(string TaskToken, string ChecklistToken, string? ItemToken, string Text) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public const int TextMaxLength = 300;

    public BoardScope GetBoardScope() => new(TaskToken: TaskToken);
}
