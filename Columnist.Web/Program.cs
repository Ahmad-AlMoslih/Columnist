using Columnist.Auth;
using Columnist.DataAccess;
using Columnist.Services;
using Columnist.Tasks;
using Columnist.Utilities;
using Columnist.Web.Application.ExceptionHandling;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Columnist.Web.Application.Authentication;
using Columnist.Auth.Security.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);   // FluentValidation validators own the required rules

builder.Services
    .AddDataAccess(builder.Configuration.GetConnectionString("Columnist")!)
    .AddServices(Path.Combine(builder.Environment.WebRootPath, "dms"), builder.Configuration["DataProtection:KeysPath"]!)
    .AddAuthorizationBehavior()
    .AddValidationBehavior()
    .AddAuthenticationBehavior()
    .AddTasksModule()
    .AddAuthModule(builder.Configuration);

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
            OnChallenge = context =>
            {
                context.HandleResponse();
                var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                context.Response.Redirect("/Auth/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl));
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
app.UseStaticFiles();   // serves runtime uploads (wwwroot/dms); MapStaticAssets only covers build-time files
app.UseRouting();

app.UseMiddleware<RefreshTokenMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapAreaControllerRoute(
    name: "tasks",
    areaName: "Tasks",
    pattern: "tasks/{action=Index}/{token?}",
    defaults: new { controller = "Tasks" });

app.MapAreaControllerRoute(
    name: "auth",
    areaName: "Auth",
    pattern: "Auth/{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();