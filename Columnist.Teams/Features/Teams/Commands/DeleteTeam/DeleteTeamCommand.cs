using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.DeleteTeam;

[RequireTeamOwner]
public sealed record DeleteTeamCommand(string TeamToken, string? ConfirmName) : IRequest<ApiResponse<bool>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(TeamToken);
}
