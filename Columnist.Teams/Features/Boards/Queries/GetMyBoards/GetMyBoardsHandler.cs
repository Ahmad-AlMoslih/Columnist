using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Teams.Features.Boards.Queries.GetMyBoards.Models;
using Columnist.Utilities.Response;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Boards.Queries.GetMyBoards;

public sealed class GetMyBoardsHandler : IRequestHandler<GetMyBoardsQuery, ApiResponse<IReadOnlyList<MyBoardModel>>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;

    public GetMyBoardsHandler(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ApiResponse<IReadOnlyList<MyBoardModel>>> Handle(GetMyBoardsQuery request, CancellationToken cancellationToken)
    {
        var teamId = request.TeamToken.Decrypt<int>();
        if (teamId is null)
            return ApiResponse<IReadOnlyList<MyBoardModel>>.Success([]);

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@TeamId", teamId.Value);

        var rows = await _db.QueryAsync<OptionDto>("dbo.GetUserBoards", parameters, cancellationToken);

        var currentId = request.CurrentBoardToken?.Decrypt<int>();
        IReadOnlyList<MyBoardModel> boards = rows
            .Select(b => new MyBoardModel
            {
                Token = b.Id.Encrypt(),
                Name = b.Name,
                IsCurrent = b.Id == currentId
            })
            .ToList();

        return ApiResponse<IReadOnlyList<MyBoardModel>>.Success(boards);
    }
}
