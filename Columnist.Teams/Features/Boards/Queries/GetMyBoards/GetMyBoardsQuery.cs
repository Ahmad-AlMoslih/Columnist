using Columnist.Teams.Features.Boards.Queries.GetMyBoards.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Boards.Queries.GetMyBoards;

public sealed record GetMyBoardsQuery(string TeamToken, string? CurrentBoardToken = null) : IRequest<ApiResponse<IReadOnlyList<MyBoardModel>>>;
