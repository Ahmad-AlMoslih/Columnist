using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Teams.Features.Teams.Queries.GetTeamLogs.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Teams.Queries.GetTeamLogs;

public sealed class GetTeamLogsHandler : IRequestHandler<GetTeamLogsQuery, ApiResponse<GetTeamLogsModel>>
{
    private const int PageSize = 20;

    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetTeamLogsHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<GetTeamLogsModel>> Handle(GetTeamLogsQuery request, CancellationToken cancellationToken)
    {
        var teamId = request.TeamToken.Decrypt<int>();
        if (teamId is null)
            return ApiResponse<GetTeamLogsModel>.NotFound(ErrorMessages.TeamNotFound);

        var userParameters = new DynamicParameters();
        userParameters.Add("@UserId", _userContext.UserId);

        var teams = await _db.QueryAsync<TeamDto>("dbo.GetUserTeams", userParameters, cancellationToken);
        var team = teams.FirstOrDefault(t => t.Id == teamId.Value && t.IsOwner);
        if (team is null)
            return ApiResponse<GetTeamLogsModel>.Forbidden(ErrorMessages.Forbidden);

        // A person token that doesn't decrypt is ignored (no person filter).
        var actorId = string.IsNullOrEmpty(request.ActorToken) ? null : request.ActorToken.Decrypt<int>();
        var search = request.Search?.Trim();

        var parameters = new DynamicParameters();
        parameters.Add("@TeamId", teamId.Value);
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@Category", string.IsNullOrEmpty(request.Category) ? null : request.Category);
        parameters.Add("@ActorId", actorId);
        parameters.Add("@Search", string.IsNullOrEmpty(search) ? null : search);
        parameters.Add("@Skip", (request.Page - 1) * PageSize);
        parameters.Add("@Take", PageSize);

        var rows = await _db.QueryAsync<AuditLogRowDto>("dbo.GetTeamLogs", parameters, cancellationToken);

        var actorParameters = new DynamicParameters();
        actorParameters.Add("@TeamId", teamId.Value);
        var actors = await _db.QueryAsync<LogActorDto>("dbo.GetLogActors", actorParameters, cancellationToken);

        var photos = await _photos.GetUrlsAsync(cancellationToken);

        IReadOnlyList<AuditEntryModel> entries = rows
            .Select(r => new AuditEntryModel
            {
                ActorName = r.ActorName,
                ActorInitials = GetInitials(r.ActorName),
                PhotoUrl = photos.GetValueOrDefault(r.ActorId),
                Action = r.Action,
                Category = r.Category,
                TargetName = r.TargetName,
                Field = r.Field,
                OldValue = r.OldValue,
                NewValue = r.NewValue,
                CreatedAt = r.CreatedAt,
                BoardName = r.BoardName
            })
            .ToList();

        return ApiResponse<GetTeamLogsModel>.Success(new GetTeamLogsModel
        {
            Entries = entries,
            TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0,
            Page = request.Page,
            Category = request.Category,
            ActorToken = actorId is null ? null : request.ActorToken,
            Search = request.Search,
            Actors = actors
                .Select(a => new TokenOptionDto { Token = a.Id.Encrypt(), Name = a.Name, Selected = a.Id == actorId })
                .ToList()
        });
    }

    // First letters of the first two words of the name, uppercase.
    private static string GetInitials(string fullName) =>
        string.Concat(fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0])));
}
