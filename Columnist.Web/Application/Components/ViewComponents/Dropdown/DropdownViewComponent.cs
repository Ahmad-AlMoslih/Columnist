using Columnist.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.Dropdown;

public sealed class DropdownViewComponent : ViewComponent
{
    private const string ViewPath = "~/Application/Components/ViewComponents/Dropdown/Default.cshtml";

    public IViewComponentResult Invoke(
        string name,
        IEnumerable<TokenOptionDto> options,
        string placeholder = "-- Choose --",
        string? requiredMessage = null)
    {
        ViewData["FieldName"] = name;
        ViewData["Placeholder"] = placeholder;
        ViewData["RequiredMessage"] = requiredMessage;
        return View(ViewPath, options);
    }
}
