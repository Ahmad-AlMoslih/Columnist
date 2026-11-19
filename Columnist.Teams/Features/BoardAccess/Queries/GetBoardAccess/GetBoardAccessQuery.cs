using Columnist.Auth.Authorization;
using Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess;

[RequireTeamOwner]
public sealed record GetBoardAccessQuery(string BoardToken, string? Search) : IRequest<ApiResponse<GetBoardAccessModel>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(BoardToken: BoardToken);
}
