using Columnist.Auth.Security.CurrentUser;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Commands.RestoreTask;

public sealed class RestoreTaskHandler : IRequestHandler<RestoreTaskCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;

    public RestoreTaskHandler(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ApiResponse<bool>> Handle(RestoreTaskCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        parameters.Add("@UserId", _userContext.UserId);

        var restored = await _db.ExecuteAsync("dbo.RestoreTask", parameters, cancellationToken);
        if (!restored)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        return ApiResponse<bool>.Success(true);
    }
}
