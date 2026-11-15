using Columnist.Auth.Security.CurrentUser;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.SaveTeam;

public sealed class SaveTeamHandler : IRequestHandler<SaveTeamCommand, ApiResponse<string>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;

    public SaveTeamHandler(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ApiResponse<string>> Handle(SaveTeamCommand request, CancellationToken cancellationToken)
    {
        var teamId = 0;
        if (!string.IsNullOrEmpty(request.TeamToken))
        {
            var decrypted = request.TeamToken.Decrypt<int>();
            if (decrypted is null)
                return ApiResponse<string>.NotFound(ErrorMessages.TeamNotFound);
            teamId = decrypted.Value;
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Id", teamId);
        parameters.Add("@Name", request.Name.Trim());
        parameters.Add("@UserId", _userContext.UserId);

        var id = await _db.QuerySingleOrDefaultAsync<int?>("dbo.SaveTeam", parameters, cancellationToken);
        if (id is null)
            return ApiResponse<string>.NotFound(ErrorMessages.TeamNotFound);

        return ApiResponse<string>.Success(id.Value.Encrypt());
    }
}
