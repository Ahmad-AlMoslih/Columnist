using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Reports.Queries.GetBoardReportCsv.Models;
using Columnist.Tasks.Features.Reports.Shared;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardReportCsv;

[RequirePermission(Permission.ReportsRead)]
public sealed record GetBoardReportCsvQuery(string BoardToken, ReportFilterRequest? Filter = null) : IRequest<ApiResponse<GetBoardReportCsvModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => new(BoardToken, null, null);
}
