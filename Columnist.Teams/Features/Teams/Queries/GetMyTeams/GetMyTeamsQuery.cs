using Columnist.Teams.Features.Teams.Queries.GetMyTeams.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Queries.GetMyTeams;

public sealed record GetMyTeamsQuery(string? CurrentTeamToken) : IRequest<ApiResponse<GetMyTeamsModel>>;
