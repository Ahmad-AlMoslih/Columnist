using System.Data;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Boards.Commands.SaveBoard;

public sealed class SaveBoardHandler : IRequestHandler<SaveBoardCommand, ApiResponse<string>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IBoardNotifier _notifier;

    public SaveBoardHandler(IDbExecutor db, IUserContext userContext, IBoardNotifier notifier)
    {
        _db = db;
        _userContext = userContext;
        _notifier = notifier;
    }

    public async Task<ApiResponse<string>> Handle(SaveBoardCommand request, CancellationToken cancellationToken)
    {
        var boardId = 0;
        if (!string.IsNullOrEmpty(request.BoardToken))
        {
            var decrypted = request.BoardToken.Decrypt<int>();
            if (decrypted is null)
                return ApiResponse<string>.NotFound(ErrorMessages.BoardNotFound);
            boardId = decrypted.Value;
        }

        var teamId = 0;
        if (boardId == 0)
        {
            var decrypted = request.TeamToken?.Decrypt<int>();
            if (decrypted is null)
                return ApiResponse<string>.NotFound(ErrorMessages.TeamNotFound);
            teamId = decrypted.Value;
        }


        var statuses = (await _db.QueryAsync<ColumnStatusDto>("dbo.GetColumnStatuses", ct: cancellationToken))
    .ToDictionary(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase);

        // dbo.BoardColumnList: Id, Name, StatusId, Position (same order).
        var columns = new DataTable();
        columns.Columns.Add("Id", typeof(int));
        columns.Columns.Add("Name", typeof(string));
        columns.Columns.Add("StatusId", typeof(int));
        columns.Columns.Add("Position", typeof(int));

        var position = 1;
        foreach (var column in request.Columns!)
        {
            var columnId = 0;
            if (!string.IsNullOrEmpty(column.Token))
            {
                var decrypted = column.Token.Decrypt<int>();
                if (decrypted is null)
                    return ApiResponse<string>.NotFound(ErrorMessages.BoardNotFound);
                columnId = decrypted.Value;
            }

            if (!statuses.TryGetValue(column.StatusName, out var statusId))
                return ApiResponse<string>.BadRequest(ErrorMessages.StatusNotFound);

            columns.Rows.Add(columnId, column.Name.Trim(), statusId, position++);
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Id", boardId);
        parameters.Add("@TeamId", teamId);
        parameters.Add("@Name", request.Name.Trim());
        parameters.Add("@Columns", columns.AsTableValuedParameter("dbo.BoardColumnList"));
        parameters.Add("@UserId", _userContext.UserId);

        var result = await _db.QuerySingleOrDefaultAsync<SaveBoardResultDto>("dbo.SaveBoard", parameters, cancellationToken);
        switch (result?.Result)
        {
            case 0:
                await _notifier.BoardChangedAsync(result.Id!.Value, cancellationToken);
                return ApiResponse<string>.Success(result.Id!.Value.Encrypt());
            case 3:
                return ApiResponse<string>.BadRequest(ErrorMessages.ColumnHasTasks);
            case 4:
                return ApiResponse<string>.BadRequest(ErrorMessages.BoardNeedsColumn);
            default:
                return ApiResponse<string>.NotFound(ErrorMessages.BoardNotFound);
        }
    }
}