using Microsoft.Extensions.DependencyInjection;

namespace Columnist.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
        services.AddScoped<IDbExecutor, DbExecutor>();
        return services;
    }
}
