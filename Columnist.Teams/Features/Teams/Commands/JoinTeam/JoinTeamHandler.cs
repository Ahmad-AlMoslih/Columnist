using Columnist.Auth.Security.CurrentUser;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.JoinTeam;

public sealed class JoinTeamHandler : IRequestHandler<JoinTeamCommand, ApiResponse<string>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;

    public JoinTeamHandler(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ApiResponse<string>> Handle(JoinTeamCommand request, CancellationToken cancellationToken)
    {
        var code = InviteCodes.Normalize(request.Code);
        if (code is null)
            return ApiResponse<string>.BadRequest(ErrorMessages.InviteCodeInvalid);

        var parameters = new DynamicParameters();
        parameters.Add("@Code", code);
        parameters.Add("@UserId", _userContext.UserId);

        var result = await _db.QuerySingleOrDefaultAsync<JoinTeamResultDto>("dbo.JoinTeam", parameters, cancellationToken);
        if (result is null || result.Result == 1)
            return ApiResponse<string>.BadRequest(ErrorMessages.InviteCodeInvalid);
        if (result.Result == 2)
            return ApiResponse<string>.BadRequest(ErrorMessages.AlreadyTeamOwner);

        return ApiResponse<string>.Success(result.TeamId!.Value.Encrypt());
    }
}
