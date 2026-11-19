using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.PageHeader;


public sealed class PageHeaderViewComponent : ViewComponent
{
    private const string ViewPath = "~/Views/Shared/Components/PageHeader/Default.cshtml";

    public Task<IViewComponentResult> InvokeAsync(string? title, string? subtitle, string? backUrl, string backLabel = "Back",
        string? backSkeleton = null, string? backSkeletonLabel = null)
    {
        ViewData["PageTitle"] = title;
        ViewData["Subtitle"] = subtitle;
        ViewData["BackUrl"] = backUrl;
        ViewData["BackLabel"] = backLabel;
        ViewData["BackSkeleton"] = backSkeleton;
        ViewData["BackSkeletonLabel"] = backSkeletonLabel;
        return Task.FromResult<IViewComponentResult>(View(ViewPath));
    }
}
