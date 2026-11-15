using System.Reflection;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Authorization;
using Columnist.Utilities.Response.Errors;
using Columnist.Utilities.Response.Interfaces;
using MediatR;

namespace Columnist.Auth.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IAuthorizationResponse<TResponse>
{
    private readonly IUserContext _userContext;
    private readonly IBoardAccessService _boards;

    public AuthorizationBehavior(IUserContext userContext, IBoardAccessService boards)
    {
        _userContext = userContext;
        _boards = boards;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var permission = typeof(TRequest).GetCustomAttribute<RequirePermissionAttribute>();
        var needsBoardAccess = typeof(TRequest).GetCustomAttribute<RequireBoardAccessAttribute>() is not null;
        if (permission is null && !needsBoardAccess)
            return await next(cancellationToken);

        var boardIds = request is IBoardRequest boardRequest
            ? await _boards.ResolveAsync(boardRequest.GetBoardScope(), cancellationToken)
            : null;

        if (boardIds is null)
        {
            // The request names no board: board access can't be checked; a permission is checked "on any board".
            if (needsBoardAccess)
                return TResponse.Forbidden(ErrorMessages.Forbidden);
            if (!await _userContext.HasPermissionAsync(permission!.Permission, cancellationToken))
                return TResponse.Forbidden(ErrorMessages.Forbidden);
            return await next(cancellationToken);
        }

        // Nothing the request points to exists: let the handler answer 404.
        if (boardIds.Count == 0)
            return await next(cancellationToken);

        foreach (var boardId in boardIds)
        {
            if (needsBoardAccess && !await _boards.CanViewAsync(boardId, cancellationToken))
                return TResponse.Forbidden(ErrorMessages.Forbidden);
            if (permission is not null && !await _boards.HasPermissionAsync(boardId, permission.Permission, cancellationToken))
                return TResponse.Forbidden(ErrorMessages.Forbidden);
        }

        return await next(cancellationToken);
    }
}
