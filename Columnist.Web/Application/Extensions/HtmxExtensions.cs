using Microsoft.AspNetCore.Http;

namespace Columnist.Web.Application.Extensions;

public static class HtmxExtensions
{
    public static bool IsHtmx(this HttpRequest request) => request.Headers["HX-Request"] == "true";
}
