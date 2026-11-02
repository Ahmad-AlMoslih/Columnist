using Columnist.Contracts.Dtos;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveTask;

public sealed record SaveTaskCommand(TaskFormDto Task) : IRequest<ApiResponse<bool>>;
