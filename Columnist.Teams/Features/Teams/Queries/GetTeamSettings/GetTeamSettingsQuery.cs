using Columnist.Auth.Authorization;
using Columnist.Teams.Features.Teams.Queries.GetTeamSettings.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Queries.GetTeamSettings;

[RequireTeamOwner]
public sealed record GetTeamSettingsQuery(string TeamToken) : IRequest<ApiResponse<GetTeamSettingsModel>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(TeamToken);
}
