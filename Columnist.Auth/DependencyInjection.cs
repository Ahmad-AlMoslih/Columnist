using Columnist.Auth.Behaviors;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Jwt;
using Columnist.Auth.Security.Passwords;
using Columnist.Auth.Security.RefreshTokens;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Columnist.Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddHttpContextAccessor();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUserContext, UserContext>();
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