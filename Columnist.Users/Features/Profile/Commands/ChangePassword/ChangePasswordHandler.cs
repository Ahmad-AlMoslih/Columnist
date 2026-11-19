using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Jwt;
using Columnist.Auth.Security.Passwords;
using Columnist.Auth.Security.RefreshTokens;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.ChangePassword;

public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<LoginResultDto>>
{
    private readonly IDbExecutor _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IUserContext _userContext;

    public ChangePasswordHandler(IDbExecutor db, IPasswordHasher passwordHasher, IJwtTokenService jwt, IRefreshTokenService refreshTokens, IUserContext userContext)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _refreshTokens = refreshTokens;
        _userContext = userContext;
    }

    public async Task<ApiResponse<LoginResultDto>> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var userId = _userContext.UserId!.Value;

        var profile = await ProfileStore.GetAsync(_db, userId, ct);
        if (profile is null)
            return ApiResponse<LoginResultDto>.NotFound();

        // Check the current password against the same row login uses (dbo.GetUserByEmail).
        var credentialsParameters = new DynamicParameters();
        credentialsParameters.Add("@Email", profile.Email);
        var credentials = await _db.QuerySingleOrDefaultAsync<UserCredentialsDto>("dbo.GetUserByEmail", credentialsParameters, ct);
        if (credentials is null || credentials.Id != userId || !_passwordHasher.Verify(request.CurrentPassword, credentials.PasswordHash))
            return ApiResponse<LoginResultDto>.BadRequest(ErrorMessages.CurrentPasswordWrong, nameof(ChangePasswordCommand.CurrentPassword));

        // One transaction: the new hash, SessionVersion + 1, every refresh token revoked, a new one for this session.
        var refreshToken = _refreshTokens.Generate();
        var refreshExpiresAt = _refreshTokens.GetExpiry();

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@PasswordHash", _passwordHasher.Hash(request.NewPassword));
        parameters.Add("@TokenHash", _refreshTokens.Hash(refreshToken));
        parameters.Add("@ExpiresAt", refreshExpiresAt);
        var sessionVersion = await _db.QuerySingleOrDefaultAsync<int?>("dbo.ChangeUserPassword", parameters, ct);
        if (sessionVersion is null)
            return ApiResponse<LoginResultDto>.ServerError();

        return ApiResponse<LoginResultDto>.Success(new LoginResultDto
        {
            Token = _jwt.CreateToken(userId, profile.Username ?? "", profile.FullName, sessionVersion.Value),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
            FullName = profile.FullName
        });
    }
}
