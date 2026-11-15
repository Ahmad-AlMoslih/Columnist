using Columnist.Teams.Features.Teams.Queries.GetTeamSettings.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Queries.GetTeamSettings;

public sealed record GetTeamSettingsQuery(string TeamToken) : IRequest<ApiResponse<GetTeamSettingsModel>>;
