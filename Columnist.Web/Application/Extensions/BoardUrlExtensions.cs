using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Columnist.Web.Application.Extensions;

public static class BoardUrlExtensions
{
    public static string BoardIndex(this IUrlHelper url, HttpRequest request, string? boardToken)
    {
        var target = url.Action("Index", "Tasks", new { area = "Tasks", boardToken })!;
        if (!url.IsBoardPage(request, out var uri))
            return target;

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("boardToken", out var shown) && !string.IsNullOrEmpty(shown) && shown != boardToken)
            return target;

        var kept = query
            .Where(pair => !pair.Key.Equals("boardToken", StringComparison.OrdinalIgnoreCase))
            .SelectMany(pair => pair.Value.Select(value => new KeyValuePair<string, string?>(pair.Key, value)));
        return QueryHelpers.AddQueryString(target, kept);
    }

    // Was the request sent from the board page (HX-Current-URL, else the Referer)?
    public static bool IsBoardPage(this IUrlHelper url, HttpRequest request) => url.IsBoardPage(request, out _);

    // The board page, unfiltered (no search or filter in its URL): every card of the board is on it.
    public static bool IsUnfilteredBoardPage(this IUrlHelper url, HttpRequest request) =>
        url.IsBoardPage(request, out var uri)
        && QueryHelpers.ParseQuery(uri.Query).All(pair => pair.Key.Equals("boardToken", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(pair.Value));

    private static bool IsBoardPage(this IUrlHelper url, HttpRequest request, [NotNullWhen(true)] out Uri? uri)
    {
        var index = url.Action("Index", "Tasks", new { area = "Tasks" })!.TrimEnd('/');
        var current = request.Headers["HX-Current-URL"].FirstOrDefault() ?? request.Headers.Referer.FirstOrDefault();
        if (!Uri.TryCreate(current, UriKind.Absolute, out uri))
            return false;

        var path = uri.AbsolutePath.TrimEnd('/');
        return path.Equals(index, StringComparison.OrdinalIgnoreCase) || path.Equals(index + "/Index", StringComparison.OrdinalIgnoreCase);
    }
}
