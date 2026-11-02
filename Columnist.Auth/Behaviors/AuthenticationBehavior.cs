using System.Reflection;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Authorization;
using Columnist.Utilities.Response.Errors;
using Columnist.Utilities.Response.Interfaces;
using MediatR;

namespace Columnist.Auth.Behaviors;
public sealed class AuthenticationBehavior<TRequest,TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IAuthorizationResponse<TResponse>
{
    private readonly IUserContext _userContext;
    public AuthenticationBehavior(IUserContext userContext)
    {
        _userContext = userContext;
    }
public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (typeof(TRequest).GetCustomAttribute<AllowAnonymousRequestAttribute>() is not null)
            return await next(cancellationToken);
        if (!_userContext.IsAuthenticated)
            return TResponse.Unauthorized(ErrorMessages.Unauthorized);
        return await next(cancellationToken);
    }
}