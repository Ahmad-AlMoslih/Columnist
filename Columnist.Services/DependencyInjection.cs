using Columnist.Services.Dms;
using Columnist.Services.Email;
using Columnist.Services.Realtime;
using Columnist.Services.Tokens.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Columnist.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration, string dmsRootPath, string keysPath)
    {
        services.AddDataProtection()
            .SetApplicationName("Columnist")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        // DmsOptions is also read on its own, e.g. for the task view's upload limits.
        var dmsOptions = new DmsOptions { RootPath = dmsRootPath };
        services.AddSingleton(dmsOptions);
        services.AddSingleton<IDmsService>(new DmsService(dmsOptions));
        services.AddSingleton<IBoardNotifier, NullBoardNotifier>();
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddSingleton<IEmailSender, EmailSender>();
        return services;
    }

    // Jwt:Key (user secrets) is the secret behind the stable keys (TokenExtensions.ToStableKey).
    public static IApplicationBuilder UseTokens(this IApplicationBuilder app)
    {
        TokenExtensions.Configure(
            app.ApplicationServices.GetRequiredService<IDataProtectionProvider>(),
            app.ApplicationServices.GetRequiredService<IConfiguration>()["Jwt:Key"]);
        return app;
    }
}
