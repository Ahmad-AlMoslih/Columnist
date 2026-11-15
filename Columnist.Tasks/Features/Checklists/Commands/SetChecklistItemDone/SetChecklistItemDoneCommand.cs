using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Commands.SetChecklistItemDone;

// Anyone who can see the board can tick items.
[RequireBoardAccess]
public sealed record SetChecklistItemDoneCommand(string TaskToken, string ChecklistToken, string ItemToken, bool Done) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: TaskToken);
}
