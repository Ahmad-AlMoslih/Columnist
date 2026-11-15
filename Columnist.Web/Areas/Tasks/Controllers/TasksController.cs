using Columnist.Contracts.Dtos;
using Columnist.Tasks.Features.Checklists.Commands.DeleteChecklist;
using Columnist.Tasks.Features.Checklists.Commands.DeleteChecklistItem;
using Columnist.Tasks.Features.Checklists.Commands.SaveChecklist;
using Columnist.Tasks.Features.Checklists.Commands.SaveChecklistItem;
using Columnist.Tasks.Features.Checklists.Commands.SetChecklistItemDone;
using Columnist.Tasks.Features.Checklists.Queries.GetTaskChecklists;
using Columnist.Tasks.Features.Tasks.Commands.DeleteTask;
using Columnist.Tasks.Features.Tasks.Commands.MoveTask;
using Columnist.Tasks.Features.Tasks.Commands.SaveTask;
using Columnist.Tasks.Features.Tasks.Commands.SaveComment;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskComments;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskForm;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskView;
using Columnist.Teams.Features.Boards.Queries.GetBoardSettings;
using Columnist.Utilities.Response.Enums;
using Columnist.Web.Application.Extensions;
using Columnist.Web.Application.Teams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Tasks.Controllers;

[Authorize]
[Area("Tasks")]

public sealed class TasksController : Controller
{
    private readonly IMediator _mediator;
    private readonly CurrentTeams _currentTeams;

    public TasksController(IMediator mediator, CurrentTeams currentTeams)
    {
        _mediator = mediator;
        _currentTeams = currentTeams;
    }

    public async Task<IActionResult> Index(string? boardToken, string? search, [FromQuery] BoardFilterRequest filter, CancellationToken ct)
    {
        var team = await TeamCookie.ResolveAsync(_currentTeams, HttpContext, ct);
        if (team is null)
            return RedirectToAction("Index", "Teams", new { area = "Teams" });

        var response = await _mediator.Send(new GetBoardQuery(boardToken, search, team, filter), ct);
        if (response.Status == ResponseStatus.NotFound && !string.IsNullOrEmpty(boardToken))
        {
            TempData["Error"] = "You don't have access to that board.";
            return RedirectToAction(nameof(Index));
        }
        if (!response.IsSuccess)
            return NotFound();

        var teams = await _currentTeams.GetAsync(ct);
        var isOwner = teams.Data?.Teams.FirstOrDefault(t => t.IsCurrent)?.IsOwner == true;
        ViewData["IsTeamOwner"] = isOwner;

        // Colours for the board's "Add another column" popup (owner only).
        if (isOwner && response.Data!.SelectedBoardToken is not null)
        {
            var settings = await _mediator.Send(new GetBoardSettingsQuery(response.Data.SelectedBoardToken, null, null), ct);
            if (settings.IsSuccess)
                ViewData["ColumnStatuses"] = settings.Data!.Statuses;
        }

        return View(response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Save(string? token, string? columnToken, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetTaskFormQuery(token, columnToken, boardToken), ct);
        if (response.Status == ResponseStatus.Forbidden)
            return StatusCode(response.StatusCode);
        if (response.Status == ResponseStatus.NotFound)
            return NotFound();

        if (Request.IsHtmx())
            return PartialView("_TaskForm", response.Data);

        return View(response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string token, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetTaskViewQuery(token), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        ViewData["BoardToken"] = boardToken;

        if (Request.IsHtmx())
            return PartialView("_TaskView", response.Data);

        return View(response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Comments(string token, string? edit, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetTaskCommentsQuery(token, edit), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        return PartialView("_TaskComments", response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveComment(string taskToken, string? commentToken, string? body, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveCommentCommand(taskToken, commentToken, body ?? ""), ct);

        if (response.IsSuccess)
            TempData["Message"] = string.IsNullOrEmpty(commentToken) ? "Comment posted." : "Comment updated.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Comments), new { token = taskToken });
    }

    // The task view's checklists region (+ #kb-toasts for hx-select-oob). add = the checklist whose "Add an item" box is open.
    [HttpGet]
    public async Task<IActionResult> Checklists(string token, string? add, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetTaskChecklistsQuery(token, add), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        ViewData["ChecklistToasts"] = true;
        return PartialView("_TaskChecklists", response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddChecklist(string taskToken, string? title, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveChecklistCommand(taskToken, null, title ?? ""), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        // A new checklist opens straight into "Add an item".
        return RedirectToAction(nameof(Checklists), new { token = taskToken, add = response.IsSuccess ? response.Data : null });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameChecklist(string taskToken, string checklistToken, string? title, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveChecklistCommand(taskToken, checklistToken, title ?? ""), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Checklists), new { token = taskToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChecklist(string taskToken, string checklistToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new DeleteChecklistCommand(taskToken, checklistToken), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Checklists), new { token = taskToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(string taskToken, string checklistToken, string? text, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveChecklistItemCommand(taskToken, checklistToken, null, text ?? ""), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        // The "Add an item" box stays open for the next item.
        return RedirectToAction(nameof(Checklists), new { token = taskToken, add = checklistToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(string taskToken, string checklistToken, string itemToken, string? text, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveChecklistItemCommand(taskToken, checklistToken, itemToken, text ?? ""), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Checklists), new { token = taskToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(string taskToken, string checklistToken, string itemToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new DeleteChecklistItemCommand(taskToken, checklistToken, itemToken), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Checklists), new { token = taskToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetItemDone(string taskToken, string checklistToken, string itemToken, bool done, CancellationToken ct)
    {
        var response = await _mediator.Send(new SetChecklistItemDoneCommand(taskToken, checklistToken, itemToken, done), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Checklists), new { token = taskToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(110_000_000)]
    public async Task<IActionResult> Save(TaskFormDto dto, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var response = await _mediator.Send(new SaveTaskCommand(dto, dto.RowVersion), ct);
            if (response.Status == ResponseStatus.Forbidden)
                return StatusCode(response.StatusCode);
            if (response.Status == ResponseStatus.NotFound)
                return NotFound();

            if (response.IsSuccess)
            {
                TempData["Message"] = dto.IsNew ? "Task created." : "Task updated.";
                return Redirect(Url.BoardIndex(Request, dto.ReturnBoardToken));
            }

            ModelState.AddApiErrors(response.Errors);
        }

        var form = await _mediator.Send(new GetTaskFormQuery(dto.Token, Posted: dto), ct);
        if (Request.IsHtmx())
        {
            Response.Headers["HX-Retarget"] = "#kb-task-modal-body";
            Response.Headers["HX-Reswap"] = "innerHTML";
            Response.Headers["HX-Reselect"] = "#kb-task-form-root";
            return PartialView("_TaskForm", form.Data);
        }

        return View(form.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string token, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new DeleteTaskCommand(token), ct);

        if (response.IsSuccess)
            TempData["Message"] = "Task deleted.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return Redirect(Url.BoardIndex(Request, boardToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(string token, string columnToken, int position, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new MoveTaskCommand(token, columnToken, position), ct);

        if (response.IsSuccess)
            TempData["Message"] = "Task moved.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return Redirect(Url.BoardIndex(Request, boardToken));
    }
}
