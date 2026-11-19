using Columnist.Auth.Behaviors;
using Columnist.Auth.Security.Boards;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Jwt;
using Columnist.Auth.Security.PasswordResets;
using Columnist.Auth.Security.Passwords;
using Columnist.Auth.Security.Permissions;
using Columnist.Auth.Security.Photos;
using Columnist.Auth.Security.RefreshTokens;
using Columnist.Auth.Security.Sessions;
using Columnist.Auth.Security.Teams;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Columnist.Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddHttpContextAccessor();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IPasswordResetTokenService, PasswordResetTokenService>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IBoardAccessService, BoardAccessService>();
        services.AddScoped<IUserTeamsService, UserTeamsService>();
        services.AddScoped<ISessionVersionService, SessionVersionService>();
        services.AddScoped<IUserPhotoService, UserPhotoService>();
        return services;
    }

    public static IServiceCollection AddAuthorizationBehavior(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>)));
        return services;
    }
    public static IServiceCollection AddAuthenticationBehavior(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(AuthenticationBehavior<,>)));
        return services;
    }
}