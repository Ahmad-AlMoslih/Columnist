using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Reports.Queries.GetBoardDashboard.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardDashboard;

[RequirePermission(Permission.ReportsRead)]
public sealed record GetBoardDashboardQuery(string BoardToken) : IRequest<ApiResponse<GetBoardDashboardModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken, null, null);
}
