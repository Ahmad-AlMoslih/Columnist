using MediatR;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard;

public sealed record GetBoardQuery(string? BoardToken, string? Search = null, string? TeamToken = null, BoardFilterRequest? Filter = null) : IRequest<ApiResponse<GetBoardModel>>;
