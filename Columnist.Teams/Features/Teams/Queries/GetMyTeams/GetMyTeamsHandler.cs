using Columnist.Auth.Security.Teams;
using Columnist.Services.Tokens.Extensions;
using Columnist.Teams.Features.Teams.Queries.GetMyTeams.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Queries.GetMyTeams;

public sealed class GetMyTeamsHandler : IRequestHandler<GetMyTeamsQuery, ApiResponse<GetMyTeamsModel>>
{
    private readonly IUserTeamsService _teams;

    public GetMyTeamsHandler(IUserTeamsService teams) => _teams = teams;

    public async Task<ApiResponse<GetMyTeamsModel>> Handle(GetMyTeamsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _teams.GetAsync(cancellationToken);
        // Current team = the cookie's team if still active, otherwise the first active team, otherwise none (lobby).
        var current = await _teams.GetCurrentAsync(request.CurrentTeamToken, cancellationToken);

        IReadOnlyList<MyTeamModel> teams = rows
            .Select(t => new MyTeamModel
            {
                Token = t.Id.Encrypt(),
                Name = t.Name,
                IsOwner = t.IsOwner,
                AccessExpiresAt = t.AccessExpiresAt,
                IsExpired = !_teams.IsActive(t),
                IsCurrent = current is not null && t.Id == current.Id
            })
            .ToList();

        return ApiResponse<GetMyTeamsModel>.Success(new GetMyTeamsModel
        {
            Teams = teams,
            CurrentTeamToken = teams.FirstOrDefault(t => t.IsCurrent)?.Token
        });
    }
}
