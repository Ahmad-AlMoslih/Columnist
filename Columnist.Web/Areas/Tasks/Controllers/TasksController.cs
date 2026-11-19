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
using Columnist.Tasks.Features.Tasks.Commands.AddTaskImages;
using Columnist.Tasks.Features.Tasks.Commands.RemoveTaskImage;
using Columnist.Tasks.Features.Tasks.Commands.SetTaskAssignees;
using Columnist.Tasks.Features.Tasks.Commands.UpdateTaskDescription;
using Columnist.Tasks.Features.Tasks.Commands.UpdateTaskDueDate;
using Columnist.Tasks.Features.Tasks.Commands.UpdateTaskTitle;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Tasks.Features.Tasks.Queries.GetBoardCard;
using Columnist.Tasks.Features.Tasks.Queries.GetBoardFilterMembers;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskComments;
using Columnist.Tasks.Features.Tasks.Queries.GetNewTaskView;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskView;
using Columnist.Teams.Features.Boards.Queries.GetMyBoards;
using Columnist.Services.Tokens.Extensions;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Enums;
using Columnist.Web.Application.Extensions;
using Columnist.Web.Application.Teams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

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
        // First load or refresh (not htmx): the shell, with no database work. It loads this same URL with htmx (Shell.cshtml).
        if (!Request.IsHtmx())
            return View("Shell");

        // The shell's own request (HX-Trigger = its id): a redirect or an error loads the whole page (HX-Redirect)
        // instead of being swapped into the shell.
        var fromShell = Request.Headers["HX-Trigger"] == "kb-board-shell";

        var team = await TeamCookie.ResolveAsync(_currentTeams, HttpContext, ct);
        if (team is null)
            return fromShell
                ? ShellRedirect(Url.Action("Index", "Teams", new { area = "Teams" })!)
                : RedirectToAction("Index", "Teams", new { area = "Teams" });

        // No board given: open the team's first board (same page, same search/filter query string).
        // No boards at all: the empty board page.
        if (string.IsNullOrEmpty(boardToken))
        {
            var boards = await _mediator.Send(new GetMyBoardsQuery(team), ct);
            var first = boards.IsSuccess ? boards.Data!.FirstOrDefault() : null;
            if (first is null)
                return View(new GetBoardModel());

            var query = Request.Query
                .Where(pair => !pair.Key.Equals("boardToken", StringComparison.OrdinalIgnoreCase))
                .SelectMany(pair => pair.Value.Select(value => new KeyValuePair<string, string?>(pair.Key, value)))
                .Prepend(new KeyValuePair<string, string?>("boardToken", first.Token));
            return Redirect(QueryHelpers.AddQueryString(Url.Action(nameof(Index))!, query));
        }

        var response = await _mediator.Send(new GetBoardQuery(boardToken, search, team, filter), ct);
        if (response.Status is ResponseStatus.NotFound or ResponseStatus.Forbidden)
        {
            TempData["Error"] = "You don't have access to that board.";
            return fromShell ? ShellRedirect(Url.Action(nameof(Index))!) : RedirectToAction(nameof(Index));
        }
        if (!response.IsSuccess)
            return fromShell ? ShellRedirect(Url.Content("~/Error/404")) : NotFound();

        return View(response.Data);
    }

    // The board filter's full assignee list (board-filter.js loads it when the dropdown first opens); the sent
    // assignees / unassigned = the panel's checked options, which stay checked.
    [HttpGet]
    public async Task<IActionResult> FilterMembers(string boardToken, string[]? assignees, string? unassigned, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetBoardFilterMembersQuery(boardToken, assignees, unassigned == "1"), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        ViewData["FilterOptionsLoaded"] = true;
        return PartialView("_BoardFilterOptions", response.Data);
    }

    // htmx loads the whole page at url (a full navigation, like a normal redirect).
    private OkResult ShellRedirect(string url)
    {
        Response.Headers["HX-Redirect"] = url;
        return Ok();
    }

    // "New task" / "+ Add task": the task view in draft mode (only the title and the column can be set).
    // Popup only: opened directly (link, new tab, refresh) it goes back to the board.
    [HttpGet]
    public async Task<IActionResult> New(string? columnToken, string? boardToken, CancellationToken ct)
    {
        if (!Request.IsHtmx())
            return Redirect(Url.BoardIndex(Request, boardToken));

        var response = await _mediator.Send(new GetNewTaskViewQuery(boardToken, columnToken), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        ViewData["BoardToken"] = boardToken;
        ViewData["EditField"] = "title";
        return PartialView("_TaskView", response.Data);
    }

    // Popup only: opened directly (link, new tab, refresh) it goes back to the board.
    [HttpGet]
    public async Task<IActionResult> Details(string token, string? boardToken, CancellationToken ct)
    {
        if (!Request.IsHtmx())
            return Redirect(Url.BoardIndex(Request, boardToken));

        var response = await _mediator.Send(new GetTaskViewQuery(token), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        ViewData["BoardToken"] = boardToken;
        return PartialView("_TaskView", response.Data);
    }

    [HttpGet]
    public Task<IActionResult> Comments(string token, string? edit, CancellationToken ct) => CommentsAsync(token, edit, ct);

    // The task view's comments region (it shows its own toasts). edit = the comment whose edit box is open.
    private async Task<IActionResult> CommentsAsync(string token, string? edit, CancellationToken ct)
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

        return await CommentsAsync(taskToken, null, ct);
    }

    [HttpGet]
    public Task<IActionResult> Checklists(string token, string? add, CancellationToken ct) => ChecklistsAsync(token, add, ct);

    // The task view's checklists region (+ #kb-toasts for hx-select-oob). add = the checklist whose "Add an item" box is open.
    private async Task<IActionResult> ChecklistsAsync(string token, string? add, CancellationToken ct)
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
        return await ChecklistsAsync(taskToken, response.IsSuccess ? response.Data : null, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameChecklist(string taskToken, string checklistToken, string? title, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveChecklistCommand(taskToken, checklistToken, title ?? ""), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return await ChecklistsAsync(taskToken, null, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChecklist(string taskToken, string checklistToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new DeleteChecklistCommand(taskToken, checklistToken), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return await ChecklistsAsync(taskToken, null, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(string taskToken, string checklistToken, string? text, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveChecklistItemCommand(taskToken, checklistToken, null, text ?? ""), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        // The "Add an item" box stays open for the next item.
        return await ChecklistsAsync(taskToken, checklistToken, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(string taskToken, string checklistToken, string itemToken, string? text, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveChecklistItemCommand(taskToken, checklistToken, itemToken, text ?? ""), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return await ChecklistsAsync(taskToken, null, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(string taskToken, string checklistToken, string itemToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new DeleteChecklistItemCommand(taskToken, checklistToken, itemToken), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return await ChecklistsAsync(taskToken, null, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetItemDone(string taskToken, string checklistToken, string itemToken, bool done, CancellationToken ct)
    {
        var response = await _mediator.Send(new SetChecklistItemDoneCommand(taskToken, checklistToken, itemToken, done), ct);
        if (!response.IsSuccess)
            TempData["Error"] = response.Errors[0].Message;

        return await ChecklistsAsync(taskToken, null, ct);
    }

    // The draft's "Create task": just the title and the column; the rest is added in place afterwards.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? title, string? columnToken, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new SaveTaskCommand(new TaskFormDto { Title = title?.Trim() ?? "", ColumnToken = columnToken }), ct);
        if (response.IsSuccess)
        {
            TempData["Message"] = "Task created.";
            return await TaskViewAsync(response.Data!, boardToken, ct);
        }
        if (response.Status == ResponseStatus.Forbidden)
            return StatusCode(response.StatusCode);

        // Draft again: a title error stays inline in the open title box, anything else is a toast.
        var draft = await _mediator.Send(new GetNewTaskViewQuery(boardToken, columnToken), ct);
        if (!draft.IsSuccess)
            return StatusCode(draft.StatusCode);

        if (response.Status == ResponseStatus.BadRequest && string.Equals(response.Errors[0].Field, nameof(TaskFormDto.Title), StringComparison.OrdinalIgnoreCase))
            ModelState.AddApiErrors(response.Errors);
        else
            TempData["Error"] = response.Errors[0].Message;

        ViewData["BoardToken"] = boardToken;
        ViewData["EditField"] = "title";
        ViewData["EditValue"] = title;
        ViewData["TaskToasts"] = true;
        return PartialView("_TaskView", draft.Data);
    }

    // In-place edits of the task view. Each returns the re-rendered _TaskView (morphed in, skipping a field still being edited).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTitle(string token, string? rowVersion, string? title, string? boardToken, CancellationToken ct) =>
        await FieldSavedAsync(await _mediator.Send(new UpdateTaskTitleCommand(token, title?.Trim() ?? "", rowVersion), ct), token, boardToken, "title", title, ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDescription(string token, string? rowVersion, string? description, string? boardToken, CancellationToken ct) =>
        await FieldSavedAsync(await _mediator.Send(new UpdateTaskDescriptionCommand(token, description, rowVersion), ct), token, boardToken, "description", description, ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDueDate(string token, string? rowVersion, DateTime? dueDate, string? boardToken, CancellationToken ct) =>
        await FieldSavedAsync(await _mediator.Send(new UpdateTaskDueDateCommand(token, dueDate, rowVersion), ct), token, boardToken, "dueDate", dueDate?.ToString("yyyy-MM-dd"), ct);

    // assigneeTokens = the people ticked in "+ Add"; remove = the chip whose × was clicked.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAssignees(string token, string? rowVersion, List<string>? assigneeTokens, string? remove, string? boardToken, CancellationToken ct)
    {
        var tokens = (assigneeTokens ?? []).Where(t => !string.IsNullOrEmpty(t) && t != remove).ToList();

        var response = await _mediator.Send(new SetTaskAssigneesCommand(token, tokens, rowVersion), ct);
        return await FieldSavedAsync(response, token, boardToken, null, null, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(110_000_000)]
    public async Task<IActionResult> AddImages(string token, string? rowVersion, List<IFormFile>? images, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new AddTaskImagesCommand(token, images ?? [], rowVersion), ct);
        if (response.IsSuccess)
            TempData["Message"] = images?.Count == 1 ? "Image uploaded." : "Images uploaded.";
        return await FieldSavedAsync(response, token, boardToken, null, null, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveImage(string token, string? rowVersion, string imageToken, string? boardToken, CancellationToken ct) =>
        await FieldSavedAsync(await _mediator.Send(new RemoveTaskImageCommand(token, imageToken, rowVersion), ct), token, boardToken, null, null, ct);

    // The column pill: the task goes to the end of the picked column.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveToColumn(string token, string? rowVersion, string columnToken, string? boardToken, CancellationToken ct) =>
        await FieldSavedAsync(await _mediator.Send(new MoveTaskCommand(token, columnToken, int.MaxValue, rowVersion), ct), token, boardToken, null, null, ct, withCard: false);

    // After a field save: a validation error re-renders that field open (field = its editor, value = what was typed)
    // with the error inline; a field without a text box (assignees, images, column), a conflict or any other failure is a toast.
    // Either way the view is re-rendered, so a conflict also reloads it. A save made on the (unfiltered) board also sends
    // the task's card out of band; withCard = false: the column change, which refreshes the whole board instead.
    private async Task<IActionResult> FieldSavedAsync(ApiResponse<bool> response, string token, string? boardToken, string? field, string? value, CancellationToken ct, bool withCard = true)
    {
        if (response.Status == ResponseStatus.Forbidden)
            return StatusCode(response.StatusCode);

        if (!response.IsSuccess)
        {
            if (response.Status == ResponseStatus.BadRequest && field is not null)
            {
                ModelState.AddApiErrors(response.Errors);
                ViewData["EditField"] = field;
                ViewData["EditValue"] = value;
            }
            else
            {
                TempData["Error"] = response.Errors[0].Message;
            }
        }
        else if (withCard && Url.IsUnfilteredBoardPage(Request))
        {
            var card = await _mediator.Send(new GetBoardCardQuery(token), ct);
            if (card.IsSuccess)
                ViewData["Card"] = card.Data;
        }

        return await TaskViewAsync(token, boardToken, ct);
    }

    // The task view with its toasts (#kb-toasts, for hx-select-oob). A task that is gone: just the toast, nothing else swapped.
    private async Task<IActionResult> TaskViewAsync(string token, string? boardToken, CancellationToken ct)
    {
        ViewData["BoardToken"] = boardToken;
        ViewData["TaskToasts"] = true;

        var view = await _mediator.Send(new GetTaskViewQuery(token), ct);
        if (view.IsSuccess)
            return PartialView("_TaskView", view.Data);
        if (view.Status == ResponseStatus.Forbidden)
            return StatusCode(view.StatusCode);

        TempData["Error"] = view.Errors[0].Message;
        Response.Headers["HX-Reswap"] = "none";
        return PartialView("_TaskToasts");
    }

    // Success = the "Task deleted." toast and, on the board, the card's removal, both out of band, with HX-Reswap: none
    // (board-dnd.js updates the counts, task-modal.js closes the popup). A failure = the error toast (#kb-toasts).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string token, CancellationToken ct)
    {
        var response = await _mediator.Send(new DeleteTaskCommand(token), ct);
        if (response.IsSuccess)
        {
            TempData["Message"] = "Task deleted.";
            ViewData["ToastsOob"] = true;
            if (Url.IsBoardPage(Request))
                ViewData["DeletedCardKey"] = token.Decrypt<int>()?.ToStableKey("task");
            Response.Headers["HX-Reswap"] = "none";
            return PartialView("_TaskDeleted");
        }

        TempData["Error"] = response.Errors[0].Message;
        return PartialView("_TaskToasts");
    }

    // One board card (board-dnd.js swaps it in after a drag into or out of a Done column). Nothing to show = 204: the card stays.
    [HttpGet]
    public async Task<IActionResult> Card(string token, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetBoardCardQuery(token), ct);
        if (!response.IsSuccess)
            return NoContent();
        ViewData["BoardToken"] = boardToken;
        return PartialView("_TaskCard", response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // The board's drag and drop. The card already sits where it was dropped: success = the "Task moved." toast as an
    // hx-swap-oob update of #kb-toasts, with HX-Reswap: none (nothing else swapped);
    // a failure = the error toast (#kb-toasts), and board-dnd.js puts the card back.
    public async Task<IActionResult> Move(string token, string columnToken, int position, CancellationToken ct)
    {
        var response = await _mediator.Send(new MoveTaskCommand(token, columnToken, position), ct);
        if (response.IsSuccess)
        {
            TempData["Message"] = "Task moved.";
            ViewData["ToastsOob"] = true;
            Response.Headers["HX-Reswap"] = "none";
            return PartialView("_TaskToasts");
        }

        TempData["Error"] = response.Errors[0].Message;
        return PartialView("_TaskToasts");
    }
}
