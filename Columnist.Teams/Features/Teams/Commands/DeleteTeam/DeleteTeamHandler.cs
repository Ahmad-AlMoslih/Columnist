using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.DeleteTeam;

public sealed class DeleteTeamHandler : IRequestHandler<DeleteTeamCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public DeleteTeamHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var teamId = request.TeamToken.Decrypt<int>();
        if (teamId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TeamNotFound);

        var userParameters = new DynamicParameters();
        userParameters.Add("@UserId", _userContext.UserId);

        var teams = await _db.QueryAsync<TeamDto>("dbo.GetUserTeams", userParameters, cancellationToken);
        var team = teams.FirstOrDefault(t => t.Id == teamId.Value && t.IsOwner);
        if (team is null)
            return ApiResponse<bool>.Forbidden(ErrorMessages.Forbidden);

        if (request.ConfirmName?.Trim() != team.Name)
            return ApiResponse<bool>.BadRequest(ErrorMessages.DeleteNameMismatch);

        var parameters = new DynamicParameters();
        parameters.Add("@TeamId", teamId.Value);
        parameters.Add("@UserId", _userContext.UserId);

        var boards = await _db.QueryAsync<TeamBoardDto>("dbo.GetTeamBoards", parameters, cancellationToken);
        if (boards.Count > 0)
            return ApiResponse<bool>.BadRequest(ErrorMessages.TeamHasBoards);

        var memberIds = (await _db.QueryAsync<TeamMemberDto>("dbo.GetTeamMembers", parameters, cancellationToken))
            .Where(m => !m.IsOwner)
            .Select(m => m.UserId)
            .ToList();

        var deleted = await _db.ExecuteAsync("dbo.DeleteTeam", parameters, cancellationToken);
        if (!deleted)
            return ApiResponse<bool>.BadRequest(ErrorMessages.TeamHasBoards);

        foreach (var memberId in memberIds)
            await _notifier.AccessChangedAsync(memberId, $"The team \"{team.Name}\" was deleted.", cancellationToken);

        return ApiResponse<bool>.Success(true);
    }
}
