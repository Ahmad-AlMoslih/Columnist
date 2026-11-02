using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.MoveTask
{
    public sealed record MoveTaskCommand(string Token, string ColumnToken) : IRequest<ApiResponse<bool>>;
}
