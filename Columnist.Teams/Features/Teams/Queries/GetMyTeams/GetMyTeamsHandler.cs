using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Teams.Features.Teams.Queries.GetMyTeams.Models;
using Columnist.Utilities.Response;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Teams.Queries.GetMyTeams;

public sealed class GetMyTeamsHandler : IRequestHandler<GetMyTeamsQuery, ApiResponse<GetMyTeamsModel>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;

    public GetMyTeamsHandler(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ApiResponse<GetMyTeamsModel>> Handle(GetMyTeamsQuery request, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", _userContext.UserId);

        var rows = await _db.QueryAsync<TeamDto>("dbo.GetUserTeams", parameters, cancellationToken);
        var now = DateTime.Now;   // the DB stores local times (SYSDATETIME)
        bool IsActive(TeamDto t) => t.IsOwner || t.AccessExpiresAt > now;

        var currentId = string.IsNullOrEmpty(request.CurrentTeamToken) ? null : request.CurrentTeamToken.Decrypt<int>();
        // Current team = the cookie's team if still active, otherwise the first active team, otherwise none (lobby).
        var current = rows.FirstOrDefault(t => t.Id == currentId && IsActive(t)) ?? rows.FirstOrDefault(IsActive);

        IReadOnlyList<MyTeamModel> teams = rows
            .Select(t => new MyTeamModel
            {
                Token = t.Id.Encrypt(),
                Name = t.Name,
                IsOwner = t.IsOwner,
                AccessExpiresAt = t.AccessExpiresAt,
                IsExpired = !IsActive(t),
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
