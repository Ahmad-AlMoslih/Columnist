using Columnist.Tasks.Features.Logs.Queries.GetBoardLogs;
using Columnist.Utilities.Response.Enums;
using Columnist.Web.Models;
using Columnist.Web.Application.Teams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Tasks.Controllers;

[Authorize]
[Area("Tasks")]
public class LogsController : Controller
{
    private readonly IMediator _mediator;
    private readonly CurrentTeams _currentTeams;

    public LogsController(IMediator mediator, CurrentTeams currentTeams)
    {
        _mediator = mediator;
        _currentTeams = currentTeams;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? board, string? category, string? actor, string? q, int page = 1, string? after = null, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(board))
            return Forbidden();

        var lastDay = DateOnly.TryParseExact(after, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var day) ? day : (DateOnly?)null;

        var response = await _mediator.Send(new GetBoardLogsQuery(board, category, actor, q, page), ct);
        if (response.Status is ResponseStatus.Forbidden or ResponseStatus.NotFound)
            return Forbidden();
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        var teams = await _currentTeams.GetAsync(ct);
        ViewData["IsTeamOwner"] = teams.Data?.Teams.FirstOrDefault(t => t.IsCurrent)?.IsOwner == true;

        return View(new LogsViewModel(board, response.Data! with { LastDay = lastDay }));
    }

    private IActionResult Forbidden()
    {
        TempData["Error"] = "You can't see these logs.";
        return RedirectToAction("Index", "Tasks");
    }
}
