using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Commands.AddBoardMember;

public sealed class AddBoardMemberHandler : IRequestHandler<AddBoardMemberCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public AddBoardMemberHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<bool>> Handle(AddBoardMemberCommand request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.BoardNotFound);

        var memberId = request.MemberToken.Decrypt<int>();
        if (memberId is null)
            return ApiResponse<bool>.BadRequest(ErrorMessages.PermissionChangeFailed);

        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId.Value);
        parameters.Add("@MemberId", memberId.Value);
        parameters.Add("@UserId", _userContext.UserId);

        var saved = await _db.ExecuteAsync("dbo.AddBoardMember", parameters, cancellationToken);
        if (!saved)
            return ApiResponse<bool>.BadRequest(ErrorMessages.PermissionChangeFailed);

        var nameParameters = new DynamicParameters();
        nameParameters.Add("@BoardId", boardId.Value);
        nameParameters.Add("@UserId", _userContext.UserId);
        var boardName = (await _db.QueryAsync<BoardSettingsRowDto>("dbo.GetBoardSettings", nameParameters, cancellationToken)).FirstOrDefault()?.BoardName ?? "a board";

        var message = $"You can now see the board \"{boardName}\".";
        await _notifier.AccessChangedAsync(memberId.Value, message, cancellationToken);
        return ApiResponse<bool>.Success(true);
    }
}