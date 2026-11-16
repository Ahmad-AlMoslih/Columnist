using Columnist.Auth.Security.PasswordResets;
using Columnist.Auth.Security.Passwords;
using Columnist.DataAccess;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Users.Features.Users.Commands.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetTokenService _resetTokens;

    public ResetPasswordHandler(IDbExecutor db, IPasswordHasher passwordHasher, IPasswordResetTokenService resetTokens)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _resetTokens = resetTokens;
    }

    public async Task<ApiResponse<bool>> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.Token))
            return ApiResponse<bool>.NotFound(ErrorMessages.ResetLinkExpired);

        // One transaction: token marked used, the new hash (the hasher login verifies with),
        // SessionVersion + 1 and every refresh token revoked. Returns 0 only when the token was valid.
        var parameters = new DynamicParameters();
        parameters.Add("@TokenHash", _resetTokens.Hash(request.Token));
        parameters.Add("@PasswordHash", _passwordHasher.Hash(request.NewPassword));
        var reset = await _db.ExecuteAsync("dbo.ResetUserPassword", parameters, ct);

        return reset
            ? ApiResponse<bool>.Success(true)
            : ApiResponse<bool>.NotFound(ErrorMessages.ResetLinkExpired);
    }
}
