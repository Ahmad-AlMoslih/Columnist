using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Jwt;
using Columnist.DataAccess;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.SaveProfile;

public sealed class SaveProfileHandler : IRequestHandler<SaveProfileCommand, ApiResponse<string>>
{
    // dbo.SaveUserProfile results.
    private const int UserNotFound = 1;
    private const int UsernameTaken = 2;

    private readonly IDbExecutor _db;
    private readonly IJwtTokenService _jwt;
    private readonly IUserContext _userContext;

    public SaveProfileHandler(IDbExecutor db, IJwtTokenService jwt, IUserContext userContext)
    {
        _db = db;
        _jwt = jwt;
        _userContext = userContext;
    }

    public async Task<ApiResponse<string>> Handle(SaveProfileCommand request, CancellationToken ct)
    {
        var userId = _userContext.UserId!.Value;

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@FullName", request.FullName.Trim());
        parameters.Add("@Username", request.Username.Trim());
        var result = await _db.QuerySingleOrDefaultAsync<int?>("dbo.SaveUserProfile", parameters, ct);

        if (result == UsernameTaken)
            return ApiResponse<string>.BadRequest(ErrorMessages.UsernameTaken, nameof(SaveProfileCommand.Username));
        if (result is null || result == UserNotFound)
            return ApiResponse<string>.NotFound();

        var profile = await ProfileStore.GetAsync(_db, userId, ct);
        if (profile is null)
            return ApiResponse<string>.NotFound();

        return ApiResponse<string>.Success(_jwt.CreateToken(userId, profile.Username ?? "", profile.FullName, profile.SessionVersion));
    }
}
