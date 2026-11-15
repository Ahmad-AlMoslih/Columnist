using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Teams.Features.Boards.Queries.GetBoardSettings.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Boards.Queries.GetBoardSettings;

public sealed class GetBoardSettingsHandler : IRequestHandler<GetBoardSettingsQuery, ApiResponse<GetBoardSettingsModel>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;

    public GetBoardSettingsHandler(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ApiResponse<GetBoardSettingsModel>> Handle(GetBoardSettingsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ColumnStatusModel> statuses = (await _db.QueryAsync<ColumnStatusDto>("dbo.GetColumnStatuses", ct: cancellationToken))
            .Select(s => new ColumnStatusModel { Id = s.Id, Name = s.Name, Color = s.Value ?? "" })
            .ToList();

        if (!string.IsNullOrEmpty(request.BoardToken))
        {
            var boardId = request.BoardToken.Decrypt<int>();
            if (boardId is null)
                return ApiResponse<GetBoardSettingsModel>.NotFound(ErrorMessages.BoardNotFound);

            var parameters = new DynamicParameters();
            parameters.Add("@BoardId", boardId.Value);
            parameters.Add("@UserId", _userContext.UserId);

            var rows = await _db.QueryAsync<BoardSettingsRowDto>("dbo.GetBoardSettings", parameters, cancellationToken);
            if (rows.Count == 0)
                return ApiResponse<GetBoardSettingsModel>.NotFound(ErrorMessages.BoardNotFound);

            return ApiResponse<GetBoardSettingsModel>.Success(new GetBoardSettingsModel
            {
                BoardToken = request.BoardToken,
                TeamToken = rows[0].TeamId.Encrypt(),
                Name = rows[0].BoardName,
                IsNew = false,
                Columns = rows
                    .Where(r => r.ColumnId is not null)
                    .Select(r => new BoardColumnSettingsModel
                    {
                        Token = r.ColumnId!.Value.Encrypt(),
                        Name = r.ColumnName!,
                        StatusId = r.StatusId!.Value,
                        TaskCount = r.TaskCount
                    })
                    .ToList(),
                Statuses = statuses
            });
        }

        var teamId = request.TeamToken?.Decrypt<int>();
        if (teamId is null)
            return ApiResponse<GetBoardSettingsModel>.NotFound(ErrorMessages.TeamNotFound);

        var userParameters = new DynamicParameters();
        userParameters.Add("@UserId", _userContext.UserId);

        var teams = await _db.QueryAsync<TeamDto>("dbo.GetUserTeams", userParameters, cancellationToken);
        if (!teams.Any(t => t.Id == teamId.Value && t.IsOwner))
            return ApiResponse<GetBoardSettingsModel>.Forbidden(ErrorMessages.Forbidden);

        return ApiResponse<GetBoardSettingsModel>.Success(new GetBoardSettingsModel
        {
            BoardToken = null,
            TeamToken = request.TeamToken!,
            Name = request.NewBoardName?.Trim() ?? "",
            IsNew = true,
            // A starting set the owner can change.
            Columns = statuses
                .Take(3)
                .Select(s => new BoardColumnSettingsModel { Token = null, Name = s.Name, StatusId = s.Id, TaskCount = 0 })
                .ToList(),
            Statuses = statuses
        });
    }
}
