using Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess;

public sealed record GetBoardAccessQuery(string BoardToken, string? Search) : IRequest<ApiResponse<GetBoardAccessModel>>;
