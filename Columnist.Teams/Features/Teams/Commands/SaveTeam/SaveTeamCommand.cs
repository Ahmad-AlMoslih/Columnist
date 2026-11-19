using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.SaveTeam;

// TeamToken empty = new team (the caller becomes its Super Admin); otherwise rename. Returns the team token.
[RequireTeamOwner]
public sealed record SaveTeamCommand(string? TeamToken, string Name) : IRequest<ApiResponse<string>>, ITeamRequest
{
    public TeamScope GetTeamScope() => new(TeamToken);

    public const int NameMaxLength = 100;   // = Teams.Name NVARCHAR(100)
}
