using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Columnist.Teams;

public static class DependencyInjection
{
    public static IServiceCollection AddTeamsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
