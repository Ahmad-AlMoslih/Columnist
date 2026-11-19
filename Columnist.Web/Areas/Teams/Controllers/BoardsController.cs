using Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess;
using Columnist.Teams.Features.Boards.Commands.DeleteBoard;
using Columnist.Teams.Features.Boards.Commands.SaveBoard;
using Columnist.Teams.Features.Boards.Queries.GetBoardSettings;
using Columnist.Web.Application.Extensions;
using Columnist.Web.Application.Teams;
using Columnist.Web.Models;
using MediatR;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Teams.Controllers;

[Authorize]
[Area("Teams")]
public class BoardsController : Controller
{
    private readonly IMediator _mediator;
    private readonly CurrentTeams _currentTeams;

    public BoardsController(IMediator mediator, CurrentTeams currentTeams)
    {
        _mediator = mediator;
        _currentTeams = currentTeams;
    }

    // token = existing board; teamToken (no token) = new board in that team. A new board only has the Columns section.
    [HttpGet]
    public async Task<IActionResult> Manage(string? token, string? teamToken, string? section, string? search, string? name, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token) || section == "columns")
        {
            var settings = await _mediator.Send(new GetBoardSettingsQuery(token, teamToken, name), ct);
            if (!settings.IsSuccess)
                return StatusCode(settings.StatusCode);

            return View(new ManageBoardViewModel(settings.Data!.BoardToken, settings.Data.IsNew ? "New board" : $"Manage {settings.Data.Name}", "columns", null, settings.Data));
        }

        var access = await _mediator.Send(new GetBoardAccessQuery(token, search), ct);
        if (!access.IsSuccess)
            return StatusCode(access.StatusCode);

        return View(new ManageBoardViewModel(access.Data!.BoardToken, $"Manage {access.Data.BoardName}", "access", access.Data, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SaveBoardCommand command, CancellationToken ct)
    {
        var response = await _mediator.Send(command, ct);
        if (response.IsSuccess)
        {
            TempData["Message"] = "Board saved.";
            return RedirectToAction(nameof(Manage), new { token = response.Data, section = "columns" });
        }

        TempData["Error"] = response.Errors[0].Message;
        if (!string.IsNullOrEmpty(command.BoardToken))
            return RedirectToAction(nameof(Manage), new { token = command.BoardToken, section = "columns" });

        return RedirectToAction(nameof(Manage), new { teamToken = command.TeamToken, name = command.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string token, string? confirmName, CancellationToken ct)
    {
        var response = await _mediator.Send(new DeleteBoardCommand(token, confirmName), ct);
        if (!response.IsSuccess)
        {
            TempData["Error"] = response.Errors[0].Message;
            return RedirectToAction(nameof(Manage), new { token, section = "columns" });
        }
        TempData["Message"] = "Board deleted.";
        return RedirectToAction("Index", "Tasks", new { area = "Tasks" });
    }

    // Board page → "Add another column": the popup's status options (add-column.js loads them when it first opens).
    [HttpGet]
    public async Task<IActionResult> ColumnStatuses(string boardToken, CancellationToken ct)
    {
        var settings = await _mediator.Send(new GetBoardSettingsQuery(boardToken, null, null), ct);
        if (!settings.IsSuccess)
            return StatusCode(settings.StatusCode);
        return PartialView("_ColumnStatusOptions", settings.Data!.Statuses);
    }

    // Board page → "Add another column": appends one column and saves the board with its full column list.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddColumn(string boardToken, string? name, string? statusName, CancellationToken ct)
    {
        var settings = await _mediator.Send(new GetBoardSettingsQuery(boardToken, null, null), ct);
        string? error = settings.IsSuccess ? null : settings.Errors[0].Message;

        if (error is null)
        {
            var board = settings.Data!;
            var columns = board.Columns
                .Select(c => new SaveBoardColumn(c.Token, c.Name, board.Statuses.FirstOrDefault(s => s.Id == c.StatusId)?.Name ?? ""))
                .Append(new SaveBoardColumn(null, name ?? "", statusName ?? ""))
                .ToList();

            var response = await _mediator.Send(new SaveBoardCommand(boardToken, board.TeamToken, board.Name, columns), ct);
            if (response.IsSuccess)
            {
                TempData["Message"] = "Column added";
                return Redirect(Url.BoardIndex(Request, boardToken));
            }
            error = response.Errors[0].Message;
        }

        if (Request.IsHtmx())
        {
            // Keep the popup open: show the error inside it.
            Response.Headers["HX-Retarget"] = "#kb-add-column-error";
            Response.Headers["HX-Reswap"] = "outerHTML";
            Response.Headers["HX-Reselect"] = "#kb-add-column-error";
            return Content($"<span id=\"kb-add-column-error\">{WebUtility.HtmlEncode(error)}</span>", "text/html");
        }

        TempData["Error"] = error;
        return Redirect(Url.BoardIndex(Request, boardToken));
    }

    // Side panel → BOARDS "＋": creates a board in the current team from the "New board" popup.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromMenu(string? name, CancellationToken ct)
    {
        var teamToken = await TeamCookie.ResolveAsync(_currentTeams, HttpContext, ct);
        var trimmed = name?.Trim() ?? "";
        string error;

        var settings = await _mediator.Send(new GetBoardSettingsQuery(null, teamToken, trimmed), ct);
        if (settings.IsSuccess)
        {
            var board = settings.Data!;
            var columns = board.Columns
                .Select(c => new SaveBoardColumn(c.Token, c.Name, board.Statuses.FirstOrDefault(s => s.Id == c.StatusId)?.Name ?? ""))
                .ToList();

            var response = await _mediator.Send(new SaveBoardCommand(null, teamToken, trimmed, columns), ct);
            if (response.IsSuccess)
            {
                TempData["Message"] = "Board created";
                var url = Url.Action("Index", "Tasks", new { area = "Tasks", boardToken = response.Data })!;
                if (!Request.IsHtmx())
                    return Redirect(url);

                Response.Headers["HX-Redirect"] = url;
                return Ok();
            }
            error = response.Errors[0].Message;
        }
        else
        {
            error = settings.Errors[0].Message;
        }

        if (!Request.IsHtmx())
        {
            TempData["Error"] = error;
            return RedirectToAction("Index", "Tasks", new { area = "Tasks" });
        }

        // Keep the popup open with the error inside it.
        return PartialView("_NewBoardForm", (Name: (string?)name, Error: (string?)error));
    }
}
