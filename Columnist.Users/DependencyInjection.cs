using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Columnist.Users;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
