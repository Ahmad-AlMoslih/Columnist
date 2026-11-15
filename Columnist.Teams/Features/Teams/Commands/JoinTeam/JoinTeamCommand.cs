using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.JoinTeam;

// Returns the team token.
public sealed record JoinTeamCommand(string Code) : IRequest<ApiResponse<string>>;
