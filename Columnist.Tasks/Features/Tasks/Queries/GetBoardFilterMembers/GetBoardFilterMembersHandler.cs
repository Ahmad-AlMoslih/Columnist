using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoardFilterMembers;

public sealed class GetBoardFilterMembersHandler : IRequestHandler<GetBoardFilterMembersQuery, ApiResponse<BoardFilterModel>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetBoardFilterMembersHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<BoardFilterModel>> Handle(GetBoardFilterMembersQuery request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<BoardFilterModel>.NotFound(ErrorMessages.BoardNotFound);

        var meId = _userContext.UserId ?? 0;
        var sentTokens = BoardFilterPeople.SentTokens(request.Assignees);
        var users = await BoardFilterPeople.GetUsersAsync(_db, boardId.Value, _userContext.UserId, cancellationToken);
        var photos = await _photos.GetUrlsAsync(cancellationToken);

        return ApiResponse<BoardFilterModel>.Success(new BoardFilterModel
        {
            MeToken = sentTokens.GetValueOrDefault(meId) ?? meId.Encrypt(),
            MeSelected = meId != 0 && sentTokens.ContainsKey(meId),
            Members = BoardFilterPeople.Members(users, meId, sentTokens, photos).ToList(),
            Unassigned = request.Unassigned
        });
    }
}
