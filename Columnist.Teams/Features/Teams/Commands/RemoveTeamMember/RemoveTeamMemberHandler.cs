using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.RemoveTeamMember;

public sealed class RemoveTeamMemberHandler : IRequestHandler<RemoveTeamMemberCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public RemoveTeamMemberHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var teamId = request.TeamToken.Decrypt<int>();
        if (teamId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TeamNotFound);

        var memberId = request.MemberToken.Decrypt<int>();
        if (memberId is null)
            return ApiResponse<bool>.BadRequest(ErrorMessages.TeamMemberNotFound);

        var userParameters = new DynamicParameters();
        userParameters.Add("@UserId", _userContext.UserId);

        var teams = await _db.QueryAsync<TeamDto>("dbo.GetUserTeams", userParameters, cancellationToken);
        var teamName = teams.FirstOrDefault(t => t.Id == teamId.Value && t.IsOwner)?.Name;
        if (teamName is null)
            return ApiResponse<bool>.Forbidden(ErrorMessages.Forbidden);

        var parameters = new DynamicParameters();
        parameters.Add("@TeamId", teamId.Value);
        parameters.Add("@MemberId", memberId.Value);
        parameters.Add("@UserId", _userContext.UserId);

        var removed = await _db.ExecuteAsync("dbo.RemoveTeamMember", parameters, cancellationToken);
        if (!removed)
            return ApiResponse<bool>.BadRequest(ErrorMessages.TeamMemberNotFound);

        await _notifier.AccessChangedAsync(memberId.Value, $"You were removed from the team \"{teamName}\".", cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}
