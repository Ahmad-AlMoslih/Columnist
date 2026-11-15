using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Jwt;
using Columnist.DataAccess;
using Columnist.Utilities.Response;
using Dapper;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.SaveFullName;

public sealed class SaveFullNameHandler : IRequestHandler<SaveFullNameCommand, ApiResponse<string>>
{
    private readonly IDbExecutor _db;
    private readonly IJwtTokenService _jwt;
    private readonly IUserContext _userContext;

    public SaveFullNameHandler(IDbExecutor db, IJwtTokenService jwt, IUserContext userContext)
    {
        _db = db;
        _jwt = jwt;
        _userContext = userContext;
    }

    public async Task<ApiResponse<string>> Handle(SaveFullNameCommand request, CancellationToken ct)
    {
        var userId = _userContext.UserId!.Value;

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@FullName", request.FullName.Trim());
        if (!await _db.ExecuteAsync("dbo.SaveUserFullName", parameters, ct))
            return ApiResponse<string>.NotFound();

        var profile = await ProfileStore.GetAsync(_db, userId, ct);
        if (profile is null)
            return ApiResponse<string>.NotFound();

        return ApiResponse<string>.Success(_jwt.CreateToken(userId, profile.Username ?? "", profile.FullName, profile.SessionVersion));
    }
}
