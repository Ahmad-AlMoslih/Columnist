namespace Columnist.Tasks.Features.Reports.Queries.GetBoardReportCsv.Models;

// The report as a CSV file: UTF-8 with a BOM.
public sealed record GetBoardReportCsvModel(byte[] Content, string FileName);
