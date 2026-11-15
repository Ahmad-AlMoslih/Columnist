using Columnist.Teams.Features.Teams.Queries.GetTeamLogs.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Queries.GetTeamLogs;

public sealed record GetTeamLogsQuery(string TeamToken, string? Category, string? ActorToken, string? Search, int Page) : IRequest<ApiResponse<GetTeamLogsModel>>;
