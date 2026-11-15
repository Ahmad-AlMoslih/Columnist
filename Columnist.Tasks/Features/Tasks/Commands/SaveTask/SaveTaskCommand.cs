using Columnist.Auth.Authorization;
using Columnist.Contracts.Dtos;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveTask;

[RequirePermission(Permission.TasksWrite)]
public sealed record SaveTaskCommand(TaskFormDto Task, string? RowVersion = null) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(ColumnToken: Task.ColumnToken, TaskToken: Task.Token);
}
