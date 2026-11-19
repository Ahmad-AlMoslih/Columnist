using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Commands.RemoveBoardMember;

[RequireTeamOwner]
public sealed record RemoveBoardMemberCommand(string BoardToken, string MemberToken) : IRequest<ApiResponse<bool>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(BoardToken: BoardToken);
}
