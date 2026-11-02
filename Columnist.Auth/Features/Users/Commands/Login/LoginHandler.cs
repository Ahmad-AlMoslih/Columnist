using Dapper;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Auth.Security.Jwt;
using Columnist.Auth.Security.Passwords;
using Columnist.Auth.Security.RefreshTokens;
using MediatR;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Auth.Features.Users.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResultDto>>
{
    private readonly IDbExecutor _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refreshTokens;

    public LoginHandler(IDbExecutor db, IPasswordHasher passwordHasher, IJwtTokenService jwt, IRefreshTokenService refreshTokens)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _refreshTokens = refreshTokens;
    }

    public async Task<ApiResponse<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUserByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return ApiResponse<LoginResultDto>.Unauthorized(ErrorMessages.InvalidCredentials);

        var refreshToken = _refreshTokens.Generate();
        var refreshExpiresAt = _refreshTokens.GetExpiry();

        var saved = await SaveRefreshTokenAsync(user.Id, _refreshTokens.Hash(refreshToken), refreshExpiresAt, cancellationToken);
        if (!saved)
            return ApiResponse<LoginResultDto>.ServerError();

        return ApiResponse<LoginResultDto>.Success(new LoginResultDto
        {
            Token = _jwt.CreateToken(user.Id, user.Username, user.FullName, user.RoleName),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
            FullName = user.FullName,
            Role = user.RoleName
        });
    }

    private async Task<UserCredentialsDto?> GetUserByUsernameAsync(string username, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Username", username);
        return await _db.QuerySingleOrDefaultAsync<UserCredentialsDto>("dbo.GetUserByUsername", parameters, ct);
    }

    private async Task<bool> SaveRefreshTokenAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@TokenHash", tokenHash);
        parameters.Add("@ExpiresAt", expiresAt);
        return await _db.ExecuteAsync("dbo.SaveRefreshToken", parameters, ct);
    }
}
