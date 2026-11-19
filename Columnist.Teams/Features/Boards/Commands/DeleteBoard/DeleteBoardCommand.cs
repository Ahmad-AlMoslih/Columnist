using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Boards.Commands.DeleteBoard;

// Returns the team token (to go back to Team settings).
[RequireTeamOwner]
public sealed record DeleteBoardCommand(string BoardToken, string? ConfirmName) : IRequest<ApiResponse<string>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(BoardToken: BoardToken);
}
