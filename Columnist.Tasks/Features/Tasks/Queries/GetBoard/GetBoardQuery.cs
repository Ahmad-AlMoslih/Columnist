using Columnist.Auth.Authorization;
using MediatR;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard;

// Always names a board (TasksController picks the first board when the page is opened without one).
[RequireBoardAccess]
public sealed record GetBoardQuery(string BoardToken, string? Search = null, string? TeamToken = null, BoardFilterRequest? Filter = null) : IRequest<ApiResponse<GetBoardModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken: BoardToken);
}
