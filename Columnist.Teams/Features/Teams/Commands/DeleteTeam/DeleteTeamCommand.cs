using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.DeleteTeam;

public sealed record DeleteTeamCommand(string TeamToken, string? ConfirmName) : IRequest<ApiResponse<bool>>;
