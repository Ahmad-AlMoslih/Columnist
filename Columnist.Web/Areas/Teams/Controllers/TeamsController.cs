using Columnist.Teams.Features.Boards.Queries.GetMyBoards;
using Columnist.Teams.Features.Boards.Queries.GetMyBoards.Models;
using Columnist.Teams.Features.Teams.Commands.DeleteTeam;
using Columnist.Teams.Features.Teams.Commands.JoinTeam;
using Columnist.Teams.Features.Teams.Commands.RemoveTeamMember;
using Columnist.Teams.Features.Teams.Commands.SaveTeam;
using Columnist.Teams.Features.Teams.Queries.GetTeamLogs;
using Columnist.Teams.Features.Teams.Queries.GetTeamSettings;
using Columnist.Web.Application.Teams;
using Columnist.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Teams.Controllers;

[Authorize]
[Area("Teams")]
public class TeamsController : Controller
{
    private readonly IMediator _mediator;
    private readonly CurrentTeams _currentTeams;

    public TeamsController(IMediator mediator, CurrentTeams currentTeams)
    {
        _mediator = mediator;
        _currentTeams = currentTeams;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var response = await _currentTeams.GetAsync(ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        if (response.Data!.CurrentTeamToken is { } current)
            TeamCookie.Set(Response, current);

        return View(response.Data);
    }

    // The burger drawer's content (loaded by HTMX each time the drawer opens).
    [HttpGet]
    public async Task<IActionResult> Menu(CancellationToken ct)
    {
        var teams = await _currentTeams.GetAsync(ct);
        if (!teams.IsSuccess)
            return StatusCode(teams.StatusCode);

        Uri.TryCreate(Request.Headers["HX-Current-URL"].ToString(), UriKind.Absolute, out var page);
        var path = page?.AbsolutePath.TrimEnd('/').ToLowerInvariant() ?? "";
        ViewData["MenuPage"] =
            path == "/teams" || path == "/teams/index" ? "lobby"
            : path.StartsWith("/teams/settings") ? "team-settings"
            : null;

        IReadOnlyList<MyBoardModel> boards = [];
        if (teams.Data!.CurrentTeamToken is { } current)
        {
            TeamCookie.Set(Response, current);

            string? currentBoard = null;
            if (page is not null)
                currentBoard = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(page.Query).TryGetValue("boardToken", out var value) ? value.ToString() : null;

            var response = await _mediator.Send(new GetMyBoardsQuery(current, currentBoard), ct);
            boards = response.Data ?? [];
        }

        return PartialView("_AppMenu", new AppMenuViewModel(teams.Data!, boards));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? name, string? boardName, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveTeamCommand(null, name ?? ""), ct);
        if (!response.IsSuccess)
        {
            TempData["Error"] = response.Errors[0].Message;
            return RedirectToAction(nameof(Index));
        }

        TeamCookie.Set(Response, response.Data!);
        TempData["Message"] = "Team created. You're its Super Admin.";
        if (!string.IsNullOrEmpty(boardName))
            return RedirectToAction("Manage", "Boards", new { area = "Teams", teamToken = response.Data, name = boardName });

        return RedirectToAction(nameof(Settings), new { token = response.Data });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(string? code, CancellationToken ct)
    {
        var response = await _mediator.Send(new JoinTeamCommand(code ?? ""), ct);

        if (response.IsSuccess)
        {
            TeamCookie.Set(Response, response.Data!);
            TempData["Message"] = "You're in. Your access lasts 30 days.";
        }
        else
        {
            TempData["Error"] = response.Errors[0].Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Switch(string token)
    {
        TeamCookie.Set(Response, token);   // Index checks it is still valid
        // Open the chosen team's boards (its first board you can open).
        return RedirectToAction("Index", "Tasks", new { area = "Tasks" });
    }

    [HttpGet]
    public async Task<IActionResult> Settings(string token, string? category, string? actor, string? q, int page = 1, string? after = null, CancellationToken ct = default)
    {
        var response = await _mediator.Send(new GetTeamSettingsQuery(token), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        var logs = await _mediator.Send(new GetTeamLogsQuery(token, category, actor, q, page), ct);
        if (!logs.IsSuccess)
            return StatusCode(logs.StatusCode);

        var lastDay = DateOnly.TryParseExact(after, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var day) ? day : (DateOnly?)null;
        ViewData["TeamLogs"] = logs.Data! with { LastDay = lastDay };

        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(string token, string? name, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveTeamCommand(token, name ?? ""), ct);

        if (response.IsSuccess)
            TempData["Message"] = "Team name saved.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Settings), new { token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(string token, string memberToken, string? name, CancellationToken ct)
    {
        var response = await _mediator.Send(new RemoveTeamMemberCommand(token, memberToken), ct);

        if (response.IsSuccess)
            TempData["Message"] = $"{name} was removed from the team. Today's invite code has changed.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Settings), new { token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string token, string? confirmName, CancellationToken ct)
    {
        var response = await _mediator.Send(new DeleteTeamCommand(token, confirmName), ct);
        if (!response.IsSuccess)
        {
            TempData["Error"] = response.Errors[0].Message;
            return RedirectToAction(nameof(Settings), new { token });
        }
        TempData["Message"] = "Team deleted.";
        return RedirectToAction(nameof(Index));
    }
}
