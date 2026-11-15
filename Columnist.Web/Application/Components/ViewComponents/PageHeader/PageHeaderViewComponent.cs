using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.PageHeader;

// Page title (+ optional subtitle) with a round back button beside it.
// backLabel says where the button goes ("Back to board"); it is the button's aria-label and tooltip.
public sealed class PageHeaderViewComponent : ViewComponent
{
    private const string ViewPath = "~/Views/Shared/Components/PageHeader/Default.cshtml";

    public Task<IViewComponentResult> InvokeAsync(string? title, string? subtitle, string? backUrl, string backLabel = "Back")
    {
        ViewData["PageTitle"] = title;
        ViewData["Subtitle"] = subtitle;
        ViewData["BackUrl"] = backUrl;
        ViewData["BackLabel"] = backLabel;
        return Task.FromResult<IViewComponentResult>(View(ViewPath));
    }
}
