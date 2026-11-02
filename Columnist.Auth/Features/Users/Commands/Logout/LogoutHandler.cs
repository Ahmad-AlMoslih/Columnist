using Columnist.Auth.Security.RefreshTokens;
using Columnist.DataAccess;
using Columnist.Utilities.Response;
using Dapper;
using MediatR;

namespace Columnist.Auth.Features.Users.Commands.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IRefreshTokenService _refreshTokens;

    public LogoutHandler(IDbExecutor db, IRefreshTokenService refreshTokens)
    {
        _db = db;
        _refreshTokens = refreshTokens;
    }

    public async Task<ApiResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return ApiResponse<bool>.Success(true);

        var parameters = new DynamicParameters();
        parameters.Add("@TokenHash", _refreshTokens.Hash(request.RefreshToken));
        await _db.ExecuteAsync("dbo.RevokeRefreshToken", parameters, cancellationToken);

        return ApiResponse<bool>.Success(true);
    }
}
