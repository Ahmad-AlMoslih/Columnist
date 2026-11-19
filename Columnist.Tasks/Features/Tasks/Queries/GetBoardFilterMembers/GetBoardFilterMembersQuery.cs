using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoardFilterMembers;

// The board filter's assignee list, loaded when its dropdown first opens. Assignees / Unassigned = the panel's checked
// options, which stay checked.
[RequireBoardAccess]
public sealed record GetBoardFilterMembersQuery(string BoardToken, string[]? Assignees, bool Unassigned) : IRequest<ApiResponse<BoardFilterModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken: BoardToken);
}
