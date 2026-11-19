using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Columnist.Web.Application.Components.Helpers;

public static class StaticAssetHelpers
{
    // A wwwroot file's fingerprinted URL from MapStaticAssets (as asp-append-version gives it), for where no tag helper
    // applies (e.g. <source srcset>). path is relative to wwwroot, e.g. "images/logo.webp".
    public static string AssetUrl(this IHtmlHelper html, string path)
    {
        var context = html.ViewContext.HttpContext;
        var assets = context.GetEndpoint()?.Metadata.GetMetadata<ResourceAssetCollection>();
        return context.Request.PathBase + "/" + (assets?[path] ?? path);
    }
}
