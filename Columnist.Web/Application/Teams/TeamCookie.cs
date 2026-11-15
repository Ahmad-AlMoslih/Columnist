using Microsoft.AspNetCore.Http;

namespace Columnist.Web.Application.Teams;

// Remembers the user's current team in this browser.
public static class TeamCookie
{
    public const string Name = "Columnist.Team";

    public static string? Get(HttpRequest request) => request.Cookies[Name];

    public static void Set(HttpResponse response, string teamToken) =>
        response.Cookies.Append(Name, teamToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = DateTimeOffset.Now.AddDays(90)
        });

    // The user's current team token (cookie team if still valid, else their first active team), saved back to the cookie.
    // Null = no active team (show the lobby).
    public static async Task<string?> ResolveAsync(CurrentTeams currentTeams, HttpContext http, CancellationToken ct)
    {
        var response = await currentTeams.GetAsync(ct);
        var team = response.IsSuccess ? response.Data!.CurrentTeamToken : null;
        if (team is not null)
            Set(http.Response, team);
        return team;
    }
}
