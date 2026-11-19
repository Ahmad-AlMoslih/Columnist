using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoardCard;

// One board card, re-rendered after a drag into or out of a Done column (its due chip and "No access" tag follow the status).
[RequireBoardAccess]
public sealed record GetBoardCardQuery(string Token) : IRequest<ApiResponse<GetBoardTaskModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
