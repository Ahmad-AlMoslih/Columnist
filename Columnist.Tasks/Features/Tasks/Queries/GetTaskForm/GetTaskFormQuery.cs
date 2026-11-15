using Columnist.Auth.Authorization;
using Columnist.Contracts.Dtos;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskForm;

[RequirePermission(Permission.TasksWrite)]
public sealed record GetTaskFormQuery(string? Token, string? ColumnToken = null, string? ReturnBoardToken = null, TaskFormDto? Posted = null) : IRequest<ApiResponse<TaskFormDto>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken: ReturnBoardToken, ColumnToken: ColumnToken, TaskToken: Token);
}
