using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Logs.Queries.GetBoardLogs.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Logs.Queries.GetBoardLogs;

[RequirePermission(Permission.LogsRead)]
public sealed record GetBoardLogsQuery(string BoardToken, string? Category, string? ActorToken, string? Search, int Page) : IRequest<ApiResponse<GetBoardLogsModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken, null, null);
}
