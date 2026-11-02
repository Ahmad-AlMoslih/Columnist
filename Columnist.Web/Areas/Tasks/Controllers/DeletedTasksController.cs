using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Tasks.Commands.RestoreTask;
using Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Tasks.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Area("Tasks")]
public class DeletedTasksController : Controller
{
    private readonly IMediator _mediator;

    public DeletedTasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetDeletedTasksQuery(search), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(string token, CancellationToken ct)
    {
        var response = await _mediator.Send(new RestoreTaskCommand(token), ct);

        if (response.IsSuccess)
            TempData["Message"] = "Task restored.";
        else
            TempData["Error"] = response.Errors[0].Message;

        return RedirectToAction(nameof(Index));
    }
}
