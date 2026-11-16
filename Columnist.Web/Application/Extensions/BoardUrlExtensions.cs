using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Columnist.Web.Application.Extensions;

public static class BoardUrlExtensions
{
    public static string BoardIndex(this IUrlHelper url, HttpRequest request, string? boardToken)
    {
        var target = url.Action("Index", "Tasks", new { area = "Tasks", boardToken })!;
        var index = url.Action("Index", "Tasks", new { area = "Tasks" })!.TrimEnd('/');

        var current = request.Headers["HX-Current-URL"].FirstOrDefault() ?? request.Headers.Referer.FirstOrDefault();
        if (!Uri.TryCreate(current, UriKind.Absolute, out var uri))
            return target;

        var path = uri.AbsolutePath.TrimEnd('/');
        if (!path.Equals(index, StringComparison.OrdinalIgnoreCase) && !path.Equals(index + "/Index", StringComparison.OrdinalIgnoreCase))
            return target;

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("boardToken", out var shown) && !string.IsNullOrEmpty(shown) && shown != boardToken)
            return target;

        var kept = query
            .Where(pair => !pair.Key.Equals("boardToken", StringComparison.OrdinalIgnoreCase))
            .SelectMany(pair => pair.Value.Select(value => new KeyValuePair<string, string?>(pair.Key, value)));
        return QueryHelpers.AddQueryString(target, kept);
    }
}
