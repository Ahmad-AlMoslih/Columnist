using Columnist.Users.Features.Profile.Commands.ChangePassword;
using Columnist.Users.Features.Profile.Commands.RemovePhoto;
using Columnist.Users.Features.Profile.Commands.SaveFullName;
using Columnist.Users.Features.Profile.Commands.SavePhoto;
using Columnist.Users.Features.Profile.Queries.GetProfile;
using Columnist.Web.Application.Authentication;
using Columnist.Web.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Auth.Controllers;

// The account menu's "Profile" popup (#kb-profile-modal in _Layout). Every action works on the signed-in user only.
// A successful POST redirects to Index, so the popup and the account menu render with the new cookie.
[Authorize]
[Area("Auth")]
public class ProfileController : Controller
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<IActionResult> Index(CancellationToken ct) => ProfileView(null, false, ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveName(string? fullName, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveFullNameCommand(fullName ?? ""), ct);
        if (!response.IsSuccess)
            return await Failed(response.Errors, fullName, false, ct);

        AuthCookie.AppendToken(Response, response.Data!);
        TempData["Message"] = "Profile saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePhoto(IFormFile? photo, CancellationToken ct)
    {
        var response = await _mediator.Send(new SavePhotoCommand(photo), ct);
        if (!response.IsSuccess)
        {
            TempData["Error"] = response.Errors[0].Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["Message"] = "Photo updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoto(CancellationToken ct)
    {
        var response = await _mediator.Send(new RemovePhotoCommand(), ct);
        TempData[response.IsSuccess ? "Message" : "Error"] = response.IsSuccess ? "Photo removed." : response.Errors[0].Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string? currentPassword, string? newPassword, string? confirmPassword, CancellationToken ct)
    {
        var response = await _mediator.Send(new ChangePasswordCommand(currentPassword ?? "", newPassword ?? "", confirmPassword ?? ""), ct);
        if (!response.IsSuccess)
            return await Failed(response.Errors, null, true, ct);

        AuthCookie.Append(Response, response.Data!);
        TempData["Message"] = "Password updated. You've been signed out on your other devices.";
        return RedirectToAction(nameof(Index));
    }

    // Field errors go under their fields; any other error is a toast.
    private async Task<IActionResult> Failed(IReadOnlyList<Columnist.Utilities.Response.Errors.ApiError> errors, string? fullName, bool passwordOpen, CancellationToken ct)
    {
        ModelState.AddApiErrors(errors.Where(e => !string.IsNullOrEmpty(e.Field)));
        var other = errors.FirstOrDefault(e => string.IsNullOrEmpty(e.Field));
        if (other is not null)
            TempData["Error"] = other.Message;

        return await ProfileView(fullName, passwordOpen, ct);
    }

    // fullName = the name as typed (kept after a failed save). passwordOpen = show the "Change password" fields.
    private async Task<IActionResult> ProfileView(string? fullName, bool passwordOpen, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetProfileQuery(), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        ViewData["PasswordOpen"] = passwordOpen;
        var model = fullName is null ? response.Data! : response.Data! with { FullName = fullName };
        return PartialView("_Profile", model);
    }
}
