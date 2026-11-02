using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.ImageGallery;

public sealed class ImageGalleryViewComponent : ViewComponent
{
    private const string ViewPath = "~/Application/Components/ViewComponents/ImageGallery/Default.cshtml";

    public IViewComponentResult Invoke(IReadOnlyList<string>? imageUrls, bool removeImages = false)
    {
        ViewData["ImageUrls"] = imageUrls ?? [];
        ViewData["RemoveImages"] = removeImages;
        return View(ViewPath);
    }
}
