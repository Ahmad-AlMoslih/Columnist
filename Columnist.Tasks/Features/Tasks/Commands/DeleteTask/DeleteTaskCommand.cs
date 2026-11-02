using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.DeleteTask
{
    public sealed record DeleteTaskCommand(string Token) : IRequest<ApiResponse<bool>>;
}