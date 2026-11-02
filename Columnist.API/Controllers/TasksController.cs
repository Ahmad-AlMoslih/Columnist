using Columnist.API.Extensions;
using Columnist.Contracts.Dtos;
using Columnist.Contracts.Requests;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Commands.SaveTask;
using Columnist.Tasks.Features.Tasks.Commands.DeleteTask;
using Columnist.Tasks.Features.Tasks.Queries.GetAllTasks;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskForm;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskById;

namespace Columnist.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return (await _mediator.Send(new GetAllTasksQuery(), ct)).ToActionResult();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        return (await _mediator.Send(new DeleteTaskCommand(id.Encrypt()), ct)).ToActionResult();
    }

    [HttpGet("form-options")]
    public async Task<IActionResult> GetFormOptions(CancellationToken ct)
    {
        return (await _mediator.Send(new GetTaskFormQuery(null), ct)).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken ct)
    {
        return (await _mediator.Send(new SaveTaskCommand(new TaskFormDto
        {
            Token = null,
            Title = request.Title.Trim(),
            Description = request.Description,
            ColumnToken = request.ColumnId.Encrypt(),
            AssigneeToken = request.AssigneeId is null ? null : request.AssigneeId.Value.Encrypt(),
            DueDate = request.DueDate
        }), ct)).ToActionResult();
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        return (await _mediator.Send(new GetTaskByIdQuery(id.Encrypt()), ct)).ToActionResult();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTaskRequest request, CancellationToken ct)
    {
        return (await _mediator.Send(new SaveTaskCommand(new TaskFormDto
        {
            Token = id.Encrypt(),
            Title = request.Title.Trim(),
            Description = request.Description,
            ColumnToken = request.ColumnId.Encrypt(),
            AssigneeToken = request.AssigneeId is null ? null : request.AssigneeId.Value.Encrypt(),
            DueDate = request.DueDate
        }), ct)).ToActionResult();
    }
}
