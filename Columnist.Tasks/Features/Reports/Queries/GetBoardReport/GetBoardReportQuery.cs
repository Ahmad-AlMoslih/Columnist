using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Reports.Queries.GetBoardReport.Models;
using Columnist.Tasks.Features.Reports.Shared;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardReport;

[RequirePermission(Permission.ReportsRead)]
public sealed record GetBoardReportQuery(string BoardToken, ReportFilterRequest? Filter = null) : IRequest<ApiResponse<GetBoardReportModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken, null, null);
}
