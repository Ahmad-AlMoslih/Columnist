using Columnist.Auth.Security.PasswordResets;
using Columnist.DataAccess;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Users.Features.Users.Queries.GetPasswordReset;

public sealed class GetPasswordResetHandler : IRequestHandler<GetPasswordResetQuery, ApiResponse<string>>
{
    private readonly IDbExecutor _db;
    private readonly IPasswordResetTokenService _resetTokens;

    public GetPasswordResetHandler(IDbExecutor db, IPasswordResetTokenService resetTokens)
    {
        _db = db;
        _resetTokens = resetTokens;
    }

    public async Task<ApiResponse<string>> Handle(GetPasswordResetQuery request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.Token))
            return ApiResponse<string>.NotFound(ErrorMessages.ResetLinkExpired);

        var parameters = new DynamicParameters();
        parameters.Add("@TokenHash", _resetTokens.Hash(request.Token));
        var email = await _db.QuerySingleOrDefaultAsync<string?>("dbo.GetPasswordResetToken", parameters, ct);

        return email is null
            ? ApiResponse<string>.NotFound(ErrorMessages.ResetLinkExpired)
            : ApiResponse<string>.Success(email);
    }
}
