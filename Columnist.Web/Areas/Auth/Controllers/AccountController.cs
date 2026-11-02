using Columnist.Auth.Features.Users.Commands.Login;
using Columnist.Auth.Features.Users.Commands.Logout;
using Columnist.Contracts.Dtos;
using Columnist.Web.Application.Authentication;
using Columnist.Web.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Auth.Controllers;

[Area("Auth")]
public class AccountController : Controller
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult Login(string? returnUrl)
        => View(new LoginFormDto { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginFormDto form)
    {
        var result = await _mediator.Send(new LoginCommand(form.Username ?? string.Empty, form.Password ?? string.Empty));

        if (!result.IsSuccess)
        {
            ModelState.AddApiErrors(result.Errors);
            return View(form);
        }

        AuthCookie.Append(Response, result.Data!);

        if (Url.IsLocalUrl(form.ReturnUrl))
            return LocalRedirect(form.ReturnUrl!);

        return RedirectToAction("Index", "Tasks", new { area = "Tasks" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(Request.Cookies[AuthCookie.RefreshName]), ct);
        AuthCookie.Delete(Response);
        return RedirectToAction("Index", "Home", new { area = "" });
    }
}
