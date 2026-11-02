using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.FileUpload;

public sealed class FileUploadViewComponent : ViewComponent
{
    private const string ViewPath = "~/Application/Components/ViewComponents/FileUpload/Default.cshtml";

    public IViewComponentResult Invoke(string name, bool hasExisting = false)
    {
        ViewData["FieldName"] = name;
        ViewData["HasExisting"] = hasExisting;
        return View(ViewPath);
    }
}
