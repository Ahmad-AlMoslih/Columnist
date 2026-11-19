using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Teams;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Users.Features.Profile.Queries.GetProfile.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Queries.GetProfile;

public sealed class GetProfileHandler : IRequestHandler<GetProfileQuery, ApiResponse<GetProfileModel>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;
    private readonly IUserTeamsService _teams;

    public GetProfileHandler(IDbExecutor db, IDmsService dms, IUserContext userContext, IUserTeamsService teams)
    {
        _db = db;
        _dms = dms;
        _userContext = userContext;
        _teams = teams;
    }

    public async Task<ApiResponse<GetProfileModel>> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var profile = await ProfileStore.GetAsync(_db, _userContext.UserId!.Value, ct);
        if (profile is null)
            return ApiResponse<GetProfileModel>.NotFound();

        var team = await _teams.GetCurrentAsync(request.TeamToken, ct);

        return ApiResponse<GetProfileModel>.Success(new GetProfileModel
        {
            FullName = profile.FullName,
            Initials = string.Concat(profile.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0]))),
            Username = profile.Username,
            Email = profile.Email,
            PhotoUrl = _dms.GetUrl(DmsFolder.Profile, profile.PhotoName),
            TeamName = team?.Name,
            RoleLabel = team is null ? null : team.IsOwner ? "Super Admin" : "Member"
        });
    }
}
