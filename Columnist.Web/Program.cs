using Columnist.Auth;
using Columnist.Users;
using Columnist.DataAccess;
using Columnist.Services;
using Columnist.Tasks;
using Columnist.Teams;
using Columnist.Utilities;
using Columnist.Web.Application.ExceptionHandling;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Columnist.Web.Application.Authentication;
using Columnist.Auth.Security.Jwt;
using Columnist.Auth.Security.Sessions;
using System.Security.Claims;
using Columnist.Services.Realtime;
using Columnist.Web.Application.Realtime;
using Columnist.Web.Application.Teams;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);   // FluentValidation validators own the required rules

builder.Services
    .AddDataAccess(builder.Configuration.GetConnectionString("Columnist")!)
    .AddServices(builder.Configuration, Path.Combine(builder.Environment.WebRootPath, "dms"), builder.Configuration["DataProtection:KeysPath"]!)
    .AddAuthorizationBehavior()
    .AddValidationBehavior()
    .AddAuthenticationBehavior()
    .AddTasksModule()
    .AddUsersModule()
    .AddTeamsModule()
    .AddAuthModule(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<CurrentTeams>();   // the user's teams, loaded once per request
builder.Services.AddSingleton<IBoardNotifier, SignalRBoardNotifier>();   // replaces the Services default

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.HttpContext.Items[AuthCookie.RefreshedTokenItem] as string
                                ?? context.Request.Cookies[AuthCookie.Name];
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var versionValue = context.Principal?.FindFirstValue("sv");
                var sessions = context.HttpContext.RequestServices.GetRequiredService<ISessionVersionService>();

                if (!int.TryParse(userIdValue, out var userId)
                    || !int.TryParse(versionValue, out var version)
                    || !await sessions.IsCurrentAsync(userId, version, context.HttpContext.RequestAborted))
                {
                    context.Fail("Session is no longer valid.");
                }
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                if (context.AuthenticateFailure is not null)
                    AuthCookie.Delete(context.Response);

                var loginUrl = "/Auth/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl);

                if (context.Request.Headers.ContainsKey("HX-Request"))
                {
                    // HTMX: return to the page the user was on, not the fragment URL
                    if (Uri.TryCreate(context.Request.Headers["HX-Current-URL"].ToString(), UriKind.Absolute, out var current))
                        loginUrl = "/Auth/Account/Login?returnUrl=" + Uri.EscapeDataString(current.PathAndQuery);

                    var statusPages = context.HttpContext.Features.Get<IStatusCodePagesFeature>();
                    if (statusPages is not null)
                        statusPages.Enabled = false;

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    // Background live refreshes fail quietly; the user's next click goes to Login.
                    if (!context.Request.Headers.ContainsKey("X-Live-Refresh"))
                        context.Response.Headers["HX-Redirect"] = loginUrl;
                }
                else
                {
                    context.Response.Redirect(loginUrl);
                }

                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.Redirect("/Error/403");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Brotli first, then Gzip, for text responses (pages, htmx partials, CSS, JS, JSON, SVG), also over HTTPS. Images aren't
// listed; SignalR (/hubs) is left out below. Build-time static files come pre-compressed from MapStaticAssets.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ["text/html", "text/css", "text/javascript", "application/javascript", "application/json", "image/svg+xml"];
});

var app = builder.Build();
app.UseTokens();

app.UseExceptionHandler("/Error/500");
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/hubs"), branch => branch.UseResponseCompression());

// Runtime uploads (wwwroot/dms); every build-time file is served by MapStaticAssets (pre-compressed, fingerprinted).
var dmsPath = Path.Combine(app.Environment.WebRootPath, "dms");
Directory.CreateDirectory(dmsPath);
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(dmsPath), RequestPath = "/dms" });
app.UseRouting();

app.UseMiddleware<RefreshTokenMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapAreaControllerRoute(
    name: "tasks",
    areaName: "Tasks",
    pattern: "tasks/{action=Index}/{token?}",
    defaults: new { controller = "Tasks" })
    .WithStaticAssets();

app.MapAreaControllerRoute(
    name: "auth",
    areaName: "Auth",
    pattern: "Auth/{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.MapAreaControllerRoute(
    name: "teams",
    areaName: "Teams",
    pattern: "teams/{action=Index}/{token?}",
    defaults: new { controller = "Teams" })
    .WithStaticAssets();

app.MapAreaControllerRoute(
    name: "boards",
    areaName: "Teams",
    pattern: "boards/{action=Manage}/{token?}",
    defaults: new { controller = "Boards" })
    .WithStaticAssets();

app.MapAreaControllerRoute(
    name: "logs",
    areaName: "Tasks",
    pattern: "logs/{action=Index}",
    defaults: new { controller = "Logs" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<BoardHub>("/hubs/board");
app.Run();