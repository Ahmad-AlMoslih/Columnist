using Columnist.Contracts.Dtos;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskForm;

public sealed record GetTaskFormQuery(string? Token, string? ColumnToken = null, string? ReturnBoardToken = null, TaskFormDto? Posted = null) : IRequest<ApiResponse<TaskFormDto>>;
