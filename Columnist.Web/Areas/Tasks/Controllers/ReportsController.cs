using Columnist.Tasks.Features.Reports.Queries.GetBoardDashboard;
using Columnist.Tasks.Features.Reports.Queries.GetBoardReport;
using Columnist.Tasks.Features.Reports.Queries.GetBoardReportCsv;
using Columnist.Tasks.Features.Reports.Queries.GetDashboardCard;
using Columnist.Tasks.Features.Reports.Shared;
using Columnist.Utilities.Response.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Areas.Tasks.Controllers;

// A board's Dashboard and Report (and the report as CSV).
[Authorize]
[Area("Tasks")]
public class ReportsController : Controller
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Dashboard(string? board, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(board))
            return Forbidden();

        var response = await _mediator.Send(new GetBoardDashboardQuery(board), ct);
        if (response.Status is ResponseStatus.Forbidden or ResponseStatus.NotFound)
            return Forbidden();
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        return View(response.Data);
    }

    // A Dashboard card's rows, for its "View all" popup (dashboard.js).
    [HttpGet]
    public async Task<IActionResult> DashboardCard(string? board, string? card, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(board) || string.IsNullOrEmpty(card))
            return NotFound();

        var response = await _mediator.Send(new GetDashboardCardQuery(board, card), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        ViewData["ReportBoard"] = board;
        return PartialView("_DashboardRows", response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? board, [FromQuery] ReportFilterRequest filter, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(board))
            return Forbidden();

        var response = await _mediator.Send(new GetBoardReportQuery(board, filter), ct);
        if (response.Status is ResponseStatus.Forbidden or ResponseStatus.NotFound)
            return Forbidden();
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        return View(response.Data);
    }

    // A Report card's full list, for its "View all" (reports.js), with the report's filter. card = "key:label"
    // (the card's data-report-card).
    [HttpGet]
    public async Task<IActionResult> Card(string? board, string? card, [FromQuery] ReportFilterRequest filter, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(board) || string.IsNullOrEmpty(card))
            return NotFound();

        var response = await _mediator.Send(new GetBoardReportQuery(board, filter), ct);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        var match = response.Data!.Groups.SelectMany(g => g.Cards).FirstOrDefault(c => $"{c.Key}:{c.Label}" == card);
        if (match is null)
            return NotFound();

        ViewData["ReportBoard"] = board;
        ViewData["ReportRowsAll"] = true;
        return PartialView("_ReportRows", match);
    }

    [HttpGet]
    public async Task<IActionResult> Export(string? board, [FromQuery] ReportFilterRequest filter, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(board))
            return Forbidden();

        var response = await _mediator.Send(new GetBoardReportCsvQuery(board, filter), ct);
        if (response.Status is ResponseStatus.Forbidden or ResponseStatus.NotFound)
            return Forbidden();
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode);

        return File(response.Data!.Content, "text/csv", response.Data.FileName);
    }

    private IActionResult Forbidden()
    {
        TempData["Error"] = "You can't see this board's reports.";
        return RedirectToAction("Index", "Tasks");
    }
}
