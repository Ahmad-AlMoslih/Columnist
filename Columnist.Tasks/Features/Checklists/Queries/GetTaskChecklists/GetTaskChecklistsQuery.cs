using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Checklists.Queries.GetTaskChecklists;

// AddItemChecklistToken = the checklist whose "Add an item" box is shown open.
[RequireBoardAccess]
public sealed record GetTaskChecklistsQuery(string TaskToken, string? AddItemChecklistToken = null) : IRequest<ApiResponse<GetTaskChecklistsModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: TaskToken);
}
