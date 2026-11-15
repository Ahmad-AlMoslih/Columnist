using Columnist.Teams.Features.BoardAccess.Commands.AddBoardMember;
using Columnist.Teams.Features.BoardAccess.Commands.RemoveBoardMember;
using Columnist.Teams.Features.BoardAccess.Commands.SetBoardMemberPermission;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Teams.Controllers;

[Authorize]
[Area("Teams")]
public class BoardAccessController : Controller
{
    private readonly IMediator _mediator;

    public BoardAccessController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string boardToken, string memberToken, string? name, string? search, CancellationToken ct)
    {
        var response = await _mediator.Send(new AddBoardMemberCommand(boardToken, memberToken), ct);
        if (response.IsSuccess)
            TempData["Message"] = $"{name} can now see this board.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction("Manage", "Boards", new { token = boardToken, section = "access", search });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string boardToken, string memberToken, string? name, string? search, CancellationToken ct)
    {
        var response = await _mediator.Send(new RemoveBoardMemberCommand(boardToken, memberToken), ct);
        if (response.IsSuccess)
            TempData["Message"] = $"{name} was removed from this board.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction("Manage", "Boards", new { token = boardToken, section = "access", search });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPermission(string boardToken, string memberToken, string permissionName, bool grant, string? name, string? label, string? search, CancellationToken ct)
    {
        var response = await _mediator.Send(new SetBoardMemberPermissionCommand(boardToken, memberToken, permissionName, grant), ct);
        if (response.IsSuccess)
            TempData["Message"] = grant ? $"\"{label}\" given to {name}." : $"\"{label}\" removed from {name}.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction("Manage", "Boards", new { token = boardToken, section = "access", search });
    }
}
