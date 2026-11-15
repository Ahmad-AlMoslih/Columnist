using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.BoardFilter;

// The board's "Filter" button and its panel (board-filter.js). Every change reloads the board from url
// with the filter and search values; the panel itself is never swapped or morphed (data-morph-skip).
// searchForm = the search box's form, sent along with the filter.
public sealed class BoardFilterViewComponent : ViewComponent
{
    private const string ViewPath = "~/Application/Components/ViewComponents/BoardFilter/Default.cshtml";

    public IViewComponentResult Invoke(BoardFilterModel filter, string url, string searchForm)
    {
        ViewData["Url"] = url;
        ViewData["SearchForm"] = searchForm;
        return View(ViewPath, filter);
    }
}
