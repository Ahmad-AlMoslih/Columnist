using Columnist.Auth.Security.Jwt;
using Columnist.Auth.Security.RefreshTokens;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Auth.Features.Users.Commands.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<LoginResultDto>>
{
    private readonly IDbExecutor _db;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refreshTokens;

    public RefreshTokenHandler(IDbExecutor db, IJwtTokenService jwt, IRefreshTokenService refreshTokens)
    {
        _db = db;
        _jwt = jwt;
        _refreshTokens = refreshTokens;
    }

    public async Task<ApiResponse<LoginResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var newRefreshToken = _refreshTokens.Generate();
        var newExpiresAt = _refreshTokens.GetExpiry();

        var parameters = new DynamicParameters();
        parameters.Add("@TokenHash", _refreshTokens.Hash(request.RefreshToken));
        parameters.Add("@NewTokenHash", _refreshTokens.Hash(newRefreshToken));
        parameters.Add("@NewExpiresAt", newExpiresAt);

        var user = await _db.QuerySingleOrDefaultAsync<UserCredentialsDto>("dbo.RotateRefreshToken", parameters, cancellationToken);
        if (user is null)
            return ApiResponse<LoginResultDto>.Unauthorized(ErrorMessages.Unauthorized);

        return ApiResponse<LoginResultDto>.Success(new LoginResultDto
        {
            Token = _jwt.CreateToken(user.Id, user.Username, user.FullName, user.RoleName),
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = newExpiresAt,
            FullName = user.FullName,
            Role = user.RoleName
        });
    }
}
