using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks;

[RequirePermission(Permission.DeletedTasksRead)]
public sealed record GetDeletedTasksQuery(string? BoardToken, string? Search = null) : IRequest<ApiResponse<GetDeletedTasksModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken: BoardToken);
}
