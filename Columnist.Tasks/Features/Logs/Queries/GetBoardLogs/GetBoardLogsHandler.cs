using Columnist.Auth.Authorization;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Logs.Queries.GetBoardLogs.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Logs.Queries.GetBoardLogs;

public sealed class GetBoardLogsHandler : IRequestHandler<GetBoardLogsQuery, ApiResponse<GetBoardLogsModel>>
{
    private const int PageSize = 20;

    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;
    private readonly IBoardAccessService _boards;

    public GetBoardLogsHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos, IBoardAccessService boards)
    {
        _boards = boards;
        _db = db;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<GetBoardLogsModel>> Handle(GetBoardLogsQuery request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<GetBoardLogsModel>.NotFound(ErrorMessages.BoardNotFound);

        // A person token that doesn't decrypt is ignored (no person filter).
        var actorId = string.IsNullOrEmpty(request.ActorToken) ? null : request.ActorToken.Decrypt<int>();
        var search = request.Search?.Trim();

        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId.Value);
        parameters.Add("@Category", string.IsNullOrEmpty(request.Category) ? null : request.Category);
        parameters.Add("@ActorId", actorId);
        parameters.Add("@Search", string.IsNullOrEmpty(search) ? null : search);
        parameters.Add("@Skip", (request.Page - 1) * PageSize);
        parameters.Add("@Take", PageSize);

        var rows = await _db.QueryAsync<AuditLogRowDto>("dbo.GetBoardLogs", parameters, cancellationToken);

        var actorParameters = new DynamicParameters();
        actorParameters.Add("@BoardId", boardId.Value);
        var actors = await _db.QueryAsync<LogActorDto>("dbo.GetLogActors", actorParameters, cancellationToken);

        var catalog = await _db.QueryAsync<PermissionDto>("dbo.GetPermissions", ct: cancellationToken);

        var boardParameters = new DynamicParameters();
        boardParameters.Add("@UserId", _userContext.UserId);
        var boardName = (await _db.QueryAsync<OptionDto>("dbo.GetUserBoards", boardParameters, cancellationToken)).FirstOrDefault(b => b.Id == boardId.Value)?.Name ?? "";

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
                NewValue = r.Action is "PermissionGranted" or "PermissionRemoved" && catalog.FirstOrDefault(p => p.Name == r.NewValue) is { } permission
                    ? PermissionRules.Label(permission)
                    : r.NewValue,
                CreatedAt = r.CreatedAt
            })
            .ToList();

        return ApiResponse<GetBoardLogsModel>.Success(new GetBoardLogsModel
        {
            CanSeeDeleted = await _boards.HasPermissionAsync(boardId.Value, Permission.DeletedTasksRead, cancellationToken),
            CanSeeLogs = await _boards.HasPermissionAsync(boardId.Value, Permission.LogsRead, cancellationToken),
            CanManage = await _boards.IsOwnerAsync(boardId.Value, cancellationToken),
            BoardToken = request.BoardToken,
            BoardName = boardName,
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
