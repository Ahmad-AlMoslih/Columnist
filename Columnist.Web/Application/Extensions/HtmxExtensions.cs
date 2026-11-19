using Microsoft.AspNetCore.Http;

namespace Columnist.Web.Application.Extensions;

public static class HtmxExtensions
{
    public static bool IsHtmx(this HttpRequest request) => request.Headers["HX-Request"] == "true";

    // A Board · Dashboard · Report tab (it swaps only #kb-tab-page): the page's content without the layout (_TabLayout).
    // A Back / Forward restore and a full page load get the whole page.
    public static bool IsTabSwap(this HttpRequest request) =>
        request.IsHtmx()
        && request.Headers["HX-Target"] == "kb-tab-page"
        && request.Headers["HX-History-Restore-Request"] != "true";
}
