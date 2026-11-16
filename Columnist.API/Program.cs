using Columnist.API.ExceptionHandling;
using Columnist.API.Extensions;
using Columnist.Auth;
using Columnist.Users;
using Columnist.Auth.Security.Jwt;
using Columnist.Auth.Security.Sessions;
using Columnist.DataAccess;
using Columnist.Services;
using Columnist.Tasks;
using Columnist.Utilities;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory = context =>
        ApiResponse<object>.BadRequest(context.ModelState
            .SelectMany(entry => entry.Value!.Errors.Select(error => new ApiError { Field = entry.Key, Message = error.ErrorMessage })))
        .ToActionResult());
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddDataAccess(builder.Configuration.GetConnectionString("Columnist")!)
    .AddServices(builder.Configuration, Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "dms"), builder.Configuration["DataProtection:KeysPath"]!)
    .AddAuthenticationBehavior()
    .AddAuthorizationBehavior()
    .AddValidationBehavior()
    .AddTasksModule()
    .AddUsersModule()
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
            OnChallenge = async context =>
            {
                context.HandleResponse();
                var response = ApiResponse<object>.Unauthorized(ErrorMessages.Unauthorized);
                context.Response.StatusCode = response.StatusCode;
                await context.Response.WriteAsJsonAsync(response, context.HttpContext.RequestAborted);
            },
            OnForbidden = async context =>
            {
                var response = ApiResponse<object>.Forbidden(ErrorMessages.Forbidden);
                context.Response.StatusCode = response.StatusCode;
                await context.Response.WriteAsJsonAsync(response, context.HttpContext.RequestAborted);
            }
        };
    });
builder.Services.AddProblemDetails();   // .NET 9: UseExceptionHandler() without a path throws at startup unless this is registered
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseTokens();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }   // lets the integration tests start the API