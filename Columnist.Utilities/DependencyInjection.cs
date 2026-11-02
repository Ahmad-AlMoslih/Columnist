using Columnist.Utilities.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Columnist.Utilities;

public static class DependencyInjection
{
    public static IServiceCollection AddValidationBehavior(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        return services;
    }
}
