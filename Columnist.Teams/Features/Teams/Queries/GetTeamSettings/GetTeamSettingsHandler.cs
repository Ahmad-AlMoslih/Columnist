using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Teams.Features.Teams.Queries.GetTeamSettings.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Teams.Queries.GetTeamSettings;

public sealed class GetTeamSettingsHandler : IRequestHandler<GetTeamSettingsQuery, ApiResponse<GetTeamSettingsModel>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetTeamSettingsHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<GetTeamSettingsModel>> Handle(GetTeamSettingsQuery request, CancellationToken cancellationToken)
    {
        var teamId = request.TeamToken.Decrypt<int>();
        if (teamId is null)
            return ApiResponse<GetTeamSettingsModel>.NotFound(ErrorMessages.TeamNotFound);

        var userParameters = new DynamicParameters();
        userParameters.Add("@UserId", _userContext.UserId);

        var teams = await _db.QueryAsync<TeamDto>("dbo.GetUserTeams", userParameters, cancellationToken);
        var team = teams.FirstOrDefault(t => t.Id == teamId.Value && t.IsOwner);
        if (team is null)
            return ApiResponse<GetTeamSettingsModel>.Forbidden(ErrorMessages.Forbidden);

        // Today's code: rotate if expired. If the new code is taken by another team, the proc keeps the old one; try again (max 5).
        InviteCodeDto? code = null;
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var codeParameters = new DynamicParameters();
            codeParameters.Add("@TeamId", teamId.Value);
            codeParameters.Add("@NewCode", InviteCodes.New());
            codeParameters.Add("@UserId", _userContext.UserId);
            code = await _db.QuerySingleOrDefaultAsync<InviteCodeDto>("dbo.GetOrRotateTeamInviteCode", codeParameters, cancellationToken);
            if (code?.InviteCode is not null && code.InviteCodeExpiresAt > DateTime.Now)
                break;
        }
        if (code?.InviteCode is null || code.InviteCodeExpiresAt is null)
            return ApiResponse<GetTeamSettingsModel>.ServerError(ErrorMessages.UnexpectedError);

        var teamParameters = new DynamicParameters();
        teamParameters.Add("@TeamId", teamId.Value);
        teamParameters.Add("@UserId", _userContext.UserId);

        var memberRows = await _db.QueryAsync<TeamMemberDto>("dbo.GetTeamMembers", teamParameters, cancellationToken);
        var boardRows = await _db.QueryAsync<TeamBoardDto>("dbo.GetTeamBoards", teamParameters, cancellationToken);

        var photos = await _photos.GetUrlsAsync(cancellationToken);
        var now = DateTime.Now;   // the DB stores local times (SYSDATETIME)

        IReadOnlyList<TeamMemberModel> members = memberRows
            .Select(m => new TeamMemberModel
            {
                UserToken = m.UserId.Encrypt(),
                FullName = m.FullName,
                Username = m.Username,
                Initials = GetInitials(m.FullName),
                PhotoUrl = photos.GetValueOrDefault(m.UserId),
                IsOwner = m.IsOwner,
                JoinedAt = m.JoinedAt,
                AccessExpiresAt = m.AccessExpiresAt,
                IsExpired = !m.IsOwner && m.AccessExpiresAt <= now,
                DaysLeft = m.IsOwner ? null : Math.Max(0, (int)Math.Ceiling((m.AccessExpiresAt!.Value - now).TotalDays)),
                BoardCount = m.BoardCount,
                TaskCount = m.TaskCount
            })
            .ToList();

        return ApiResponse<GetTeamSettingsModel>.Success(new GetTeamSettingsModel
        {
            Token = request.TeamToken,
            Name = team.Name,
            InviteCode = code.InviteCode,
            InviteCodeExpiresAt = code.InviteCodeExpiresAt.Value,
            Members = members,
            BoardCount = boardRows.Count
        });
    }

    // First letters of the first two words of the name, uppercase.
    private static string GetInitials(string fullName) =>
        string.Concat(fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0])));
}
