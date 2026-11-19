using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Commands.RestoreTask;
using Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks;
using Columnist.Teams.Features.Boards.Queries.GetMyBoards;
using Columnist.Utilities.Response.Enums;
using Columnist.Web.Application.Teams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Tasks.Controllers;

[Authorize]
[Area("Tasks")]
public class DeletedTasksController : Controller
{
    private readonly IMediator _mediator;
    private readonly CurrentTeams _currentTeams;

    public DeletedTasksController(IMediator mediator, CurrentTeams currentTeams)
    {
        _mediator = mediator;
        _currentTeams = currentTeams;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? boardToken, string? search, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(boardToken))
            return RedirectToAction("Index", "Tasks");

        var response = await _mediator.Send(new GetDeletedTasksQuery(boardToken, search), ct);
        if (response.Status is ResponseStatus.Forbidden or ResponseStatus.NotFound)
        {
            TempData["Error"] = "You can't see the deleted tasks of that board.";
            return RedirectToAction("Index", "Tasks", new { boardToken });
        }

        var teams = await _currentTeams.GetAsync(ct);

        // Board name for the page subtitle.
        if (teams.Data?.CurrentTeamToken is { } teamToken)
        {
            var boards = await _mediator.Send(new GetMyBoardsQuery(teamToken, boardToken), ct);
            ViewData["BoardName"] = boards.Data?.FirstOrDefault(b => b.IsCurrent)?.Name;
        }

        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(string token, CancellationToken ct)
    {
        var response = await _mediator.Send(new RestoreTaskCommand(token), ct);
        if (response.IsSuccess)
        {
            TempData["Message"] = "Task restored.";
            ViewData["ToastsOob"] = true;
            ViewData["RestoredKey"] = token.Decrypt<int>()?.ToStableKey("task");
            Response.Headers["HX-Reswap"] = "none";
            return PartialView("_TaskRestored");
        }

        TempData["Error"] = response.Errors[0].Message;
        return PartialView("~/Areas/Tasks/Views/Tasks/_TaskToasts.cshtml");
    }
}
