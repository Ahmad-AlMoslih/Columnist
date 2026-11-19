using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Commands.SetBoardMemberPermission;

[RequireTeamOwner]
public sealed record SetBoardMemberPermissionCommand(string BoardToken, string MemberToken, string PermissionName, bool Grant) : IRequest<ApiResponse<bool>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(BoardToken: BoardToken);
}
