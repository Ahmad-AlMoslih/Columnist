using Dapper;
using Columnist.DataAccess;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Services.Tokens.Extensions;
using MediatR;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands.MoveTask;

public sealed class MoveTaskHandler : IRequestHandler<MoveTaskCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;

    public MoveTaskHandler(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ApiResponse<bool>> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        var taskId = request.Token.Decrypt<int>();
        if (taskId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var columnId = request.ColumnToken.Decrypt<int>();
        if (columnId is null)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        var parameters = new DynamicParameters();
        parameters.Add("@Id", taskId.Value);
        parameters.Add("@ColumnId", columnId.Value);
        parameters.Add("@UserId", _userContext.UserId);
        var moved = await _db.ExecuteAsync("dbo.MoveTask", parameters, cancellationToken);
        if (!moved)
            return ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound);

        return ApiResponse<bool>.Success(true);
    }
}
