using Columnist.Auth.Authorization;
using Columnist.Teams.Features.Boards.Queries.GetBoardSettings.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Boards.Queries.GetBoardSettings;

// BoardToken empty = new board in TeamToken's team.
[RequireTeamOwner]
public sealed record GetBoardSettingsQuery(string? BoardToken, string? TeamToken, string? NewBoardName) : IRequest<ApiResponse<GetBoardSettingsModel>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(TeamToken, BoardToken);
}
