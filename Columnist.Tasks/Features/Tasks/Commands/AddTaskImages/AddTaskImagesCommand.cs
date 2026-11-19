using Columnist.Auth.Authorization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Commands.AddTaskImages;

[RequirePermission(Permission.TasksWrite)]
public sealed record AddTaskImagesCommand(string Token, IReadOnlyList<IFormFile> Images, string? RowVersion) : IRequest<ApiResponse<bool>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(TaskToken: Token);
}
