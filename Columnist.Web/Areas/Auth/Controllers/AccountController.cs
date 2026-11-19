using Columnist.Users.Features.Users.Commands.ForgotPassword;
using Columnist.Users.Features.Users.Commands.Login;
using Columnist.Users.Features.Users.Commands.Logout;
using Columnist.Users.Features.Users.Commands.ResetPassword;
using Columnist.Users.Features.Users.Queries.GetPasswordReset;
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
        var result = await _mediator.Send(new LoginCommand(form.Email ?? string.Empty, form.Password ?? string.Empty));

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

    [HttpGet]
    public IActionResult ForgotPassword()
        => View(new ForgotPasswordFormDto());

    // Shows the same "Check your email" screen whether or not the email exists or an email was sent.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordFormDto form, CancellationToken ct)
    {
        var resetPageUrl = Url.Action(nameof(ResetPassword), "Account", new { area = "Auth" }, Request.Scheme)!;
        var result = await _mediator.Send(new ForgotPasswordCommand(form.Email ?? string.Empty, resetPageUrl), ct);

        if (!result.IsSuccess)
        {
            ModelState.AddApiErrors(result.Errors);
            return View(form);
        }

        return View("ForgotPasswordSent");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string? token, CancellationToken ct)
    {
        var reset = await _mediator.Send(new GetPasswordResetQuery(token ?? string.Empty), ct);
        if (!reset.IsSuccess)
            return View("ResetPasswordExpired");

        return View(new ResetPasswordFormDto { Token = token, Email = reset.Data });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordFormDto form, CancellationToken ct)
    {
        var token = form.Token ?? string.Empty;
        var result = await _mediator.Send(new ResetPasswordCommand(token, form.NewPassword ?? string.Empty, form.ConfirmPassword ?? string.Empty), ct);

        if (result.IsSuccess)
        {
            AuthCookie.Delete(Response);
            return View("ResetPasswordDone");
        }

        if (result.StatusCode == StatusCodes.Status404NotFound)
            return View("ResetPasswordExpired");

        // Validation errors: show the form again, with the email reloaded from the token.
        var reset = await _mediator.Send(new GetPasswordResetQuery(token), ct);
        if (!reset.IsSuccess)
            return View("ResetPasswordExpired");

        ModelState.AddApiErrors(result.Errors);
        return View(new ResetPasswordFormDto { Token = token, Email = reset.Data });
    }
}
