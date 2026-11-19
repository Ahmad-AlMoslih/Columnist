using Columnist.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.SearchBox;

// clientOnly = no request: just the box, marked data-search-client, for a script that filters a list in the browser
// (url, target and hiddenFields are then unused). include = more elements to send with the search (hx-include).
// skeleton = the _Skeletons template shown in the target while a slow search loads (skeletonLabel = its aria-label).
public sealed class SearchBoxViewComponent : ViewComponent
{
    private const string ViewPath = "~/Application/Components/ViewComponents/SearchBox/Default.cshtml";

    public IViewComponentResult Invoke(
        string? url,
        string? target,
        string? value,
        string placeholder,
        IReadOnlyDictionary<string, string?>? hiddenFields = null,
        int minLength = SearchRules.MinLength,
        bool clientOnly = false,
        string? include = null,
        string? skeleton = null,
        string? skeletonLabel = null)
    {
        ViewData["Url"] = url;
        ViewData["Target"] = target;
        ViewData["Value"] = value;
        ViewData["Placeholder"] = placeholder;
        ViewData["HiddenFields"] = hiddenFields ?? new Dictionary<string, string?>();
        ViewData["MinLength"] = minLength;
        ViewData["ClientOnly"] = clientOnly;
        ViewData["Include"] = include;
        ViewData["Skeleton"] = skeleton;
        ViewData["SkeletonLabel"] = skeletonLabel;
        return View(ViewPath);
    }
}
