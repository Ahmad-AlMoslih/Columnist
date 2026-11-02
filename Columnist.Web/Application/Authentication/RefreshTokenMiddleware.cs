using System.IdentityModel.Tokens.Jwt;
using Columnist.Auth.Features.Users.Commands.RefreshToken;
using MediatR;

namespace Columnist.Web.Application.Authentication;

public sealed class RefreshTokenMiddleware
{
    private readonly RequestDelegate _next;

    public RefreshTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMediator mediator)
    {
        var refreshToken = context.Request.Cookies[AuthCookie.RefreshName];

        if (!string.IsNullOrEmpty(refreshToken) && !HasLiveAccessToken(context.Request.Cookies[AuthCookie.Name]))
        {
            var result = await mediator.Send(new RefreshTokenCommand(refreshToken), context.RequestAborted);

            if (result.IsSuccess)
            {
                AuthCookie.Append(context.Response, result.Data!);
                context.Items[AuthCookie.RefreshedTokenItem] = result.Data!.Token;
            }
            else
            {
                AuthCookie.Delete(context.Response);
            }
        }

        await _next(context);
    }

    private static bool HasLiveAccessToken(string? token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (string.IsNullOrEmpty(token) || !handler.CanReadToken(token))
            return false;

        return handler.ReadJwtToken(token).ValidTo > DateTime.UtcNow;
    }
}
