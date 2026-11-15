using Columnist.Auth.Authorization;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;
using Columnist.Utilities.Constants;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess;

public sealed class GetBoardAccessHandler : IRequestHandler<GetBoardAccessQuery, ApiResponse<GetBoardAccessModel>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetBoardAccessHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<GetBoardAccessModel>> Handle(GetBoardAccessQuery request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<GetBoardAccessModel>.NotFound(ErrorMessages.BoardNotFound);

        var settingsParameters = new DynamicParameters();
        settingsParameters.Add("@BoardId", boardId.Value);
        settingsParameters.Add("@UserId", _userContext.UserId);

        // Owner-only: no rows = not found or not the Super Admin.
        var settings = await _db.QueryAsync<BoardSettingsRowDto>("dbo.GetBoardSettings", settingsParameters, cancellationToken);
        if (settings.Count == 0)
            return ApiResponse<GetBoardAccessModel>.NotFound(ErrorMessages.BoardNotFound);
        var teamId = settings[0].TeamId;

        var teamParameters = new DynamicParameters();
        teamParameters.Add("@TeamId", teamId);
        teamParameters.Add("@UserId", _userContext.UserId);

        var boards = await _db.QueryAsync<TeamBoardDto>("dbo.GetTeamBoards", teamParameters, cancellationToken);

        IReadOnlyList<PermissionDto> catalog = await _db.QueryAsync<PermissionDto>("dbo.GetPermissions", ct: cancellationToken);

        var search = request.Search?.Trim();

        var accessParameters = new DynamicParameters();
        accessParameters.Add("@BoardId", boardId.Value);
        accessParameters.Add("@Search", search is { Length: >= SearchRules.MinLength } ? search : null);
        accessParameters.Add("@UserId", _userContext.UserId);

        var rows = await _db.QueryAsync<BoardAccessRowDto>("dbo.GetBoardAccess", accessParameters, cancellationToken);

        var candidateParameters = new DynamicParameters();
        candidateParameters.Add("@BoardId", boardId.Value);
        candidateParameters.Add("@Search", null);
        candidateParameters.Add("@UserId", _userContext.UserId);

        var candidateRows = await _db.QueryAsync<BoardCandidateDto>("dbo.GetBoardCandidates", candidateParameters, cancellationToken);

        var photos = await _photos.GetUrlsAsync(cancellationToken);
        var now = DateTime.Now;   // the DB stores local times (SYSDATETIME)

        IReadOnlyList<BoardMemberModel> members = rows
           .GroupBy(r => r.UserId)
           .Select(g =>
           {
               var first = g.First();
               var permissionIds = g.Where(r => r.PermissionId is not null).Select(r => r.PermissionId!.Value).ToHashSet();
               return new BoardMemberModel
               {
                   UserToken = g.Key.Encrypt(),
                   FullName = first.FullName,
                   Username = first.Username,
                   Initials = GetInitials(first.FullName),
                   PhotoUrl = photos.GetValueOrDefault(g.Key),
                   IsExpired = first.AccessExpiresAt <= now,
                   DaysLeft = DaysUntil(first.AccessExpiresAt, now),
                   AssignedTaskCount = first.AssignedTaskCount,
                   Chips = catalog
                       .Where(p => permissionIds.Contains(p.Id))
                       .Select(p => new BoardPermissionChipModel { PermissionName = p.Name, Label = PermissionRules.Label(p) })
                       .ToList()
               };
           })
           .ToList();

        IReadOnlyList<BoardCandidateModel> candidates = candidateRows
            .Select(c => new BoardCandidateModel
            {
                UserToken = c.UserId.Encrypt(),
                FullName = c.FullName,
                Initials = GetInitials(c.FullName),
                PhotoUrl = photos.GetValueOrDefault(c.UserId),
                IsExpired = c.AccessExpiresAt <= now
            })
            .ToList();

        IReadOnlyList<BoardPermissionModel> permissions = catalog
            .Select(p =>
            {
                var read = PermissionRules.ReadFor(p, catalog);
                return new BoardPermissionModel
                {
                    Name = p.Name,
                    Label = PermissionRules.Label(p),
                    Description = p.Description,
                    RequiresName = read?.Name,
                    RequiresLabel = read is null ? null : PermissionRules.Label(read)
                };
            })
            .ToList();

        return ApiResponse<GetBoardAccessModel>.Success(new GetBoardAccessModel
        {
            BoardToken = request.BoardToken,
            BoardName = boards.FirstOrDefault(b => b.Id == boardId)?.Name,
            Search = request.Search,
            Members = members,
            Candidates = candidates,
            Permissions = permissions
        });
    }

    private static int DaysUntil(DateTime? expiresAt, DateTime now) =>
        expiresAt is { } value ? Math.Max(0, (int)Math.Ceiling((value - now).TotalDays)) : 0;

    // First letters of the first two words of the name, uppercase.
    private static string GetInitials(string fullName) =>
        string.Concat(fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0])));
}
