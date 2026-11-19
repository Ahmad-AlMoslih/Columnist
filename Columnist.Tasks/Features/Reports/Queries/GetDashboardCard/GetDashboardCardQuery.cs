using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Reports.Shared.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Reports.Queries.GetDashboardCard;

// One Dashboard card's rows, for its "View all" popup. Card = the card's key ("overdue", "week", … "activity").
[RequirePermission(Permission.ReportsRead)]
public sealed record GetDashboardCardQuery(string BoardToken, string Card) : IRequest<ApiResponse<ReportCardModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken, null, null);
}
