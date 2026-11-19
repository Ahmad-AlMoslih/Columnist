using Columnist.Teams.Features.BoardAccess.Commands.AddBoardMember;
using Columnist.Teams.Features.BoardAccess.Commands.RemoveBoardMember;
using Columnist.Teams.Features.BoardAccess.Commands.SetBoardMemberPermission;
using Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess;
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

        return await AccessAsync(boardToken, search, ct);
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

        return await AccessAsync(boardToken, search, ct);
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

        return await AccessAsync(boardToken, search, ct);
    }

    // The Manage page's access region (#ba-results) with the toasts (#kb-toasts, for hx-select-oob), read after the change.
    private async Task<IActionResult> AccessAsync(string boardToken, string? search, CancellationToken ct)
    {
        var access = await _mediator.Send(new GetBoardAccessQuery(boardToken, search), ct);
        if (!access.IsSuccess)
            return StatusCode(access.StatusCode);

        ViewData["AccessToasts"] = true;
        return PartialView("_Access", access.Data);
    }
}
