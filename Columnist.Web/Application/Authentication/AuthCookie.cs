using Columnist.Contracts.Dtos;

namespace Columnist.Web.Application.Authentication;

public static class AuthCookie
{
    public const string Name = "Columnist.Auth";
    public const string RefreshName = "Columnist.Refresh";
    public const string RefreshedTokenItem = "Columnist.RefreshedToken";

    private static CookieOptions Options(DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        IsEssential = true,
        Expires = expires
    };

    public static void Append(HttpResponse response, LoginResultDto result)
    {
        response.Cookies.Append(Name, result.Token, Options());
        response.Cookies.Append(RefreshName, result.RefreshToken, Options(result.RefreshTokenExpiresAt));
    }

    // Replaces only the JWT; the refresh token is kept.
    public static void AppendToken(HttpResponse response, string token) =>
        response.Cookies.Append(Name, token, Options());

    public static void Delete(HttpResponse response)
    {
        response.Cookies.Delete(Name, Options());
        response.Cookies.Delete(RefreshName, Options());
    }
}