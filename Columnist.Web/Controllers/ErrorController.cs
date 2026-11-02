using Columnist.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Controllers;

public sealed class ErrorController : Controller
{
    [Route("Error/{statusCode:int}")]
    public IActionResult Index(int statusCode)
    {
        Response.StatusCode = statusCode;
        return View(ErrorPageModel.For(statusCode));
    }
}
