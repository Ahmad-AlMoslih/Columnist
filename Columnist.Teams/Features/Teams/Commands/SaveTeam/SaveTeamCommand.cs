using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.SaveTeam;

// TeamToken empty = new team (the caller becomes its Super Admin); otherwise rename. Returns the team token.
public sealed record SaveTeamCommand(string? TeamToken, string Name) : IRequest<ApiResponse<string>>
{
    public const int NameMaxLength = 100;   // = Teams.Name NVARCHAR(100)
}
