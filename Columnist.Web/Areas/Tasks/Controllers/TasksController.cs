using Columnist.Contracts.Dtos;
using Columnist.Tasks.Features.Tasks.Commands.DeleteTask;
using Columnist.Tasks.Features.Tasks.Commands.MoveTask;
using Columnist.Tasks.Features.Tasks.Commands.SaveTask;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskForm;
using Columnist.Utilities.Response.Enums;
using Columnist.Web.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Tasks.Controllers;

[Authorize]
[Area("Tasks")]

public sealed class TasksController : Controller
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    public async Task<IActionResult> Index(string? boardToken, string? search, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetBoardQuery(boardToken, search), ct);
        if (!response.IsSuccess)
            return NotFound();

        return View(response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Save(string? token, string? columnToken, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetTaskFormQuery(token, columnToken, boardToken), ct);
        if (response.Status == ResponseStatus.NotFound)
            return NotFound();

        if (Request.IsHtmx())
            return PartialView("_TaskForm", response.Data);

        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(110_000_000)]
    public async Task<IActionResult> Save(TaskFormDto dto, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var response = await _mediator.Send(new SaveTaskCommand(dto), ct);
            if (response.Status == ResponseStatus.NotFound)
                return NotFound();

            if (response.IsSuccess)
            {
                TempData["Message"] = dto.IsNew ? "Task created." : "Task updated.";
                return RedirectToAction(nameof(Index), new { boardToken = dto.ReturnBoardToken });
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

        return RedirectToAction(nameof(Index), new { boardToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(string token, string columnToken, string? boardToken, CancellationToken ct)
    {
        var response = await _mediator.Send(new MoveTaskCommand(token, columnToken), ct);

        if (response.IsSuccess)
            TempData["Message"] = "Task moved.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Index), new { boardToken });
    }
}
