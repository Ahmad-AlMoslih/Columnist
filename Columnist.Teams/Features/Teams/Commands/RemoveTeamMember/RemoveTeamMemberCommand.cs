using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.RemoveTeamMember;

[RequireTeamOwner]
public sealed record RemoveTeamMemberCommand(string TeamToken, string MemberToken) : IRequest<ApiResponse<bool>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(TeamToken);
}
