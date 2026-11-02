using System.Reflection;
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

    public AuthorizationBehavior(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var attribute = typeof(TRequest).GetCustomAttribute<RequireRoleAttribute>();
        if (attribute is null)
            return await next(cancellationToken);


        if (_userContext.Role is null || !attribute.Roles.Contains(_userContext.Role))
            return TResponse.Forbidden(ErrorMessages.Forbidden);

        return await next(cancellationToken);
    }
}
