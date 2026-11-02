using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Columnist.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, string dmsRootPath, string keysPath)
    {
        services.AddDataProtection()
            .SetApplicationName("Columnist")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        services.AddSingleton<IDmsService>(new DmsService(new DmsOptions { RootPath = dmsRootPath }));
        return services;
    }

    public static IApplicationBuilder UseTokens(this IApplicationBuilder app)
    {
        TokenExtensions.Configure(app.ApplicationServices.GetRequiredService<IDataProtectionProvider>());
        return app;
    }
}
