using System.Globalization;
using System.Text;
using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Reports.Queries.GetBoardReportCsv.Models;
using Columnist.Tasks.Features.Reports.Shared;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using MediatR;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardReportCsv;

// What the report shows under the current filters: one row per task, an empty line, then "Activity (last 7 days)".
public sealed class GetBoardReportCsvHandler : IRequestHandler<GetBoardReportCsvQuery, ApiResponse<GetBoardReportCsvModel>>
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetBoardReportCsvHandler(IDbExecutor db, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<GetBoardReportCsvModel>> Handle(GetBoardReportCsvQuery request, CancellationToken cancellationToken)
    {
        var boardId = request.BoardToken.Decrypt<int>();
        if (boardId is null)
            return ApiResponse<GetBoardReportCsvModel>.NotFound(ErrorMessages.BoardNotFound);

        var data = await BoardReportData.LoadAsync(_db, _photos, boardId.Value, _userContext.UserId, cancellationToken);
        var filter = ReportFilter.Read(request.Filter, data);
        var activity = await BoardActivity.LoadAsync(_db, data, boardId.Value, filter.AssigneeId, cancellationToken);
        var today = data.Today;

        var columnOrder = data.Columns.Select((c, index) => (c.ColumnId, index)).ToDictionary(c => c.ColumnId, c => c.index);
        var tasks = data.Tasks
            .Where(filter.Matches)
            .OrderBy(t => columnOrder.GetValueOrDefault(t.ColumnId, int.MaxValue))
            .ThenBy(t => t.DueDate is null).ThenBy(t => t.DueDate).ThenBy(t => t.Title, StringComparer.OrdinalIgnoreCase);

        var csv = new StringBuilder();
        Line(csv, "Task", "Column", "Status", "Assignees", "Due date", "Overdue", "Days late", "Days in column", "Last updated",
            "Checklist", "Completed on", "Completed by", "Created");
        foreach (var t in tasks)
            Line(csv,
                t.Title,
                t.ColumnName,
                t.StatusName,
                string.Join("; ", t.Assignees.Select(a => a.FullName)),
                Date(t.DueDate),
                t.IsOverdue(today) ? "Yes" : "No",
                t.DaysLate(today)?.ToString(CultureInfo.InvariantCulture),
                t.DaysInColumn(today).ToString(CultureInfo.InvariantCulture),
                Date(DateOnly.FromDateTime(t.LastUpdatedAt)),
                t.ChecklistTotal > 0 ? $"{t.ChecklistDone}/{t.ChecklistTotal}" : null,
                Date(t.CompletedOn),
                t.CompletedByName,
                Date(DateOnly.FromDateTime(t.CreatedAt)));

        csv.Append("\r\n");
        Line(csv, $"Activity (last {ActivityRules.Days} days)");
        Line(csv, "Person", "Changes", "Moves", "Edits", "Comments", "Checklist ticks", "Tasks created");
        foreach (var (person, counts) in activity)
            Line(csv, person.Name, Number(counts.Changes), Number(counts.Moves), Number(counts.Edits), Number(counts.Comments), Number(counts.Ticks), Number(counts.Created));

        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        var boardName = string.Concat(data.BoardName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '-' : c)).Trim();
        var fileName = $"{(boardName.Length == 0 ? "board" : boardName)}-report-{today.ToString(DateFormat, CultureInfo.InvariantCulture)}.csv";

        return ApiResponse<GetBoardReportCsvModel>.Success(new GetBoardReportCsvModel(content, fileName));
    }

    private static string? Date(DateOnly? date) => date?.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    // Quotes a value when it holds a comma, a quote or a line break (quotes doubled).
    private static void Line(StringBuilder csv, params string?[] values)
    {
        csv.AppendJoin(',', values.Select(v => v is null ? "" : v.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? $"\"{v.Replace("\"", "\"\"")}\"" : v));
        csv.Append("\r\n");
    }
}
