using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Auth.Security.Teams;
using Columnist.Users.Features.Profile.Queries.GetAccountMenu.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Queries.GetAccountMenu;

public sealed class GetAccountMenuHandler : IRequestHandler<GetAccountMenuQuery, ApiResponse<GetAccountMenuModel>>
{
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;
    private readonly IUserTeamsService _teams;

    public GetAccountMenuHandler(IUserContext userContext, IUserPhotoService photos, IUserTeamsService teams)
    {
        _userContext = userContext;
        _photos = photos;
        _teams = teams;
    }

    public async Task<ApiResponse<GetAccountMenuModel>> Handle(GetAccountMenuQuery request, CancellationToken ct)
    {
        var photoUrl = _userContext.UserId is int userId ? (await _photos.GetUrlsAsync(ct)).GetValueOrDefault(userId) : null;
        var team = await _teams.GetCurrentAsync(request.TeamToken, ct);

        return ApiResponse<GetAccountMenuModel>.Success(new GetAccountMenuModel
        {
            PhotoUrl = photoUrl,
            TeamName = team?.Name,
            RoleLabel = team is null ? null : team.IsOwner ? "Super Admin" : "Member"
        });
    }
}
