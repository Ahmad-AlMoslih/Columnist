using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Boards.Commands.DeleteBoard;

public sealed class DeleteBoardHandler : IRequestHandler<DeleteBoardCommand, ApiResponse<string>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IDmsService _dms;
    private readonly IBoardNotifier _notifier;

    public DeleteBoardHandler(IDbExecutor db, IUserContext userContext, IDmsService dms, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _dms = dms;
        _notifier = notifier;
    }

    public async Task<ApiResponse<string>> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<string>.NotFound(ErrorMessages.BoardNotFound);

        var settingsParameters = new DynamicParameters();
        settingsParameters.Add("@BoardId", boardId.Value);
        settingsParameters.Add("@UserId", _userContext.UserId);

        var rows = await _db.QueryAsync<BoardSettingsRowDto>("dbo.GetBoardSettings", settingsParameters, cancellationToken);
        if (rows.Count == 0)
            return ApiResponse<string>.NotFound(ErrorMessages.BoardNotFound);

        var boardName = rows[0].BoardName;
        var teamId = rows[0].TeamId;
        if (request.ConfirmName?.Trim() != boardName)
            return ApiResponse<string>.BadRequest(ErrorMessages.DeleteNameMismatch);

        var accessParameters = new DynamicParameters();
        accessParameters.Add("@BoardId", boardId.Value);
        accessParameters.Add("@Search", null);
        accessParameters.Add("@UserId", _userContext.UserId);

        var memberIds = (await _db.QueryAsync<BoardAccessRowDto>("dbo.GetBoardAccess", accessParameters, cancellationToken))
            .Select(r => r.UserId)
            .Distinct()
            .ToList();

        var deleteParameters = new DynamicParameters();
        deleteParameters.Add("@BoardId", boardId.Value);
        deleteParameters.Add("@UserId", _userContext.UserId);

        var deleteRows = await _db.QueryAsync<DeleteBoardRowDto>("dbo.DeleteBoard", deleteParameters, cancellationToken);
        if (deleteRows.Count == 0 || deleteRows[0].Result != 0)
            return ApiResponse<string>.NotFound(ErrorMessages.BoardNotFound);
        var files = deleteRows.Where(r => r.FileName is not null).Select(r => r.FileName!).ToList();

        // Image files go only after the DB delete worked.
        _dms.Delete(DmsFolder.Task, files);

        foreach (var memberId in memberIds)
            await _notifier.AccessChangedAsync(memberId, $"The board \"{boardName}\" was deleted.", cancellationToken);
        await _notifier.BoardChangedAsync(boardId.Value, cancellationToken);

        return ApiResponse<string>.Success(teamId.Encrypt());
    }
}
