using Columnist.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.SearchBox;

public sealed class SearchBoxViewComponent : ViewComponent
{
    private const string ViewPath = "~/Application/Components/ViewComponents/SearchBox/Default.cshtml";

    public IViewComponentResult Invoke(
        string url,
        string target,
        string? value,
        string placeholder,
        IReadOnlyDictionary<string, string?>? hiddenFields = null,
        int minLength = SearchRules.MinLength)
    {
        ViewData["Url"] = url;
        ViewData["Target"] = target;
        ViewData["Value"] = value;
        ViewData["Placeholder"] = placeholder;
        ViewData["HiddenFields"] = hiddenFields ?? new Dictionary<string, string?>();
        ViewData["MinLength"] = minLength;
        return View(ViewPath);
    }
}
