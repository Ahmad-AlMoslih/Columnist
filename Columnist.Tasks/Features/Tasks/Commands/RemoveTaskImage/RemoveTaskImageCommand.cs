using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.RemoveTaskImage;

// ImageToken: the encrypted TaskImages.Id.
[RequirePermission(Permission.TasksWrite)]
public sealed record RemoveTaskImageCommand(string Token, string ImageToken, string? RowVersion) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
