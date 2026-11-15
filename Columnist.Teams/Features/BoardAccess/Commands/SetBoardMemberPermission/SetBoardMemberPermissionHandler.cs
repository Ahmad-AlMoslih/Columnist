using Columnist.Auth.Authorization;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Commands.SetBoardMemberPermission;

public sealed class SetBoardMemberPermissionHandler : IRequestHandler<SetBoardMemberPermissionCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public SetBoardMemberPermissionHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(SetBoardMemberPermissionCommand request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.BoardNotFound);

        var memberId = request.MemberToken.Decrypt<int>();
        if (memberId is null)
            return ApiResponse<bool>.BadRequest(ErrorMessages.PermissionChangeFailed);

        var catalog = await _db.QueryAsync<PermissionDto>("dbo.GetPermissions", ct: cancellationToken);
        var permission = catalog.FirstOrDefault(p => p.Name == request.PermissionName);
        if (permission is null)
            return ApiResponse<bool>.BadRequest(ErrorMessages.PermissionChangeFailed);

        var names = catalog.ToDictionary(p => p.Id, p => p.Name);

        var accessParameters = new DynamicParameters();
        accessParameters.Add("@BoardId", boardId.Value);
        accessParameters.Add("@Search", null);
        accessParameters.Add("@UserId", _userContext.UserId);

        var current = (await _db.QueryAsync<BoardAccessRowDto>("dbo.GetBoardAccess", accessParameters, cancellationToken))
               .Where(r => r.UserId == memberId.Value && r.PermissionId is int id && names.ContainsKey(id))
            .Select(r => names[r.PermissionId!.Value])
            .ToHashSet();

        var error = PermissionRules.Check(permission, request.Grant, current, catalog);
        if (error is not null)
            return ApiResponse<bool>.BadRequest(error);

        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId.Value);
        parameters.Add("@MemberId", memberId.Value);
        parameters.Add("@PermissionId", permission.Id);
        parameters.Add("@Grant", request.Grant);
        parameters.Add("@UserId", _userContext.UserId);

        var saved = await _db.ExecuteAsync("dbo.SetBoardMemberPermission", parameters, cancellationToken);
        if (!saved)
            return ApiResponse<bool>.BadRequest(ErrorMessages.PermissionChangeFailed);

        var nameParameters = new DynamicParameters();
        nameParameters.Add("@BoardId", boardId.Value);
        nameParameters.Add("@UserId", _userContext.UserId);
        var boardName = (await _db.QueryAsync<BoardSettingsRowDto>("dbo.GetBoardSettings", nameParameters, cancellationToken)).FirstOrDefault()?.BoardName ?? "a board";
        var label = PermissionRules.Label(permission);
        var message = request.Grant ? $"You were given \"{label}\" on \"{boardName}\"." : $"\"{label}\" was removed from you on \"{boardName}\".";
        await _notifier.AccessChangedAsync(memberId.Value, message, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}
