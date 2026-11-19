using System.Globalization;
using System.Text.RegularExpressions;
using Columnist.Contracts.Dtos;
using Columnist.Tasks.Features.Reports.Shared.Models;

namespace Columnist.Tasks.Features.Reports.Shared;

// Builds the cards' rows and texts (the dashboard, the report and the CSV share them).
public static partial class ReportRows
{
    private const string Ink = "#1F2A44";

    public static ReportTaskRowModel Row(ReportTask task, DateOnly today, string? detail = null) => new()
    {
        Token = task.Token,
        Title = task.Title,
        ColumnName = task.ColumnName,
        ColumnColor = Color(task.StatusColor),
        DueText = task.DueDate switch
        {
            null => null,
            { } due when !task.IsDone && due < today => $"Overdue · {Date(due)}",
            { } due when !task.IsDone && due == today => $"Today · {Date(due)}",
            { } due => Date(due)
        },
        DueTone = task.DueDate switch
        {
            { } due when !task.IsDone && due < today => ReportTone.Red,
            { } due when !task.IsDone && due == today => ReportTone.Amber,
            _ => null
        },
        ChecklistDone = task.ChecklistDone,
        ChecklistTotal = task.ChecklistTotal,
        Detail = detail,
        Assignees = task.Assignees
    };

    // Soonest due first, tasks without a due date last.
    public static IEnumerable<ReportTask> ByDue(this IEnumerable<ReportTask> tasks) =>
        tasks.OrderBy(t => t.DueDate is null).ThenBy(t => t.DueDate).ThenBy(t => t.Title, StringComparer.OrdinalIgnoreCase);

    // A status colour as #RRGGBB, or null when it isn't one.
    public static string? Color(string statusColor) => HexColor().IsMatch(statusColor) ? statusColor.ToUpperInvariant() : null;

    // A column's card: its status colour as the tint and that colour mixed 60/40 with the ink as the foreground.
    public static ReportTone ForStatus(string statusColor) =>
        Color(statusColor) is { } color ? new ReportTone(Mix(color, Ink, 0.6), color) : ReportTone.Grey;

    // "today", "1 day ago", "5 days ago"
    public static string DaysAgo(int days) => days == 0 ? "today" : $"{Days(days)} ago";

    public static ReportPersonModel Person(UserOptionDto user, IReadOnlyDictionary<int, string> photos) => new()
    {
        FullName = user.Name,
        Initials = Initials(user.Name),
        PhotoUrl = photos.GetValueOrDefault(user.Id),
        HasAccess = user.HasAccess
    };

    public static ReportPersonModel Person(int userId, string fullName, IReadOnlyDictionary<int, string> photos) => new()
    {
        FullName = fullName,
        Initials = Initials(fullName),
        PhotoUrl = photos.GetValueOrDefault(userId)
    };

    // "Nov 12"
    public static string Date(DateOnly date) => date.ToString("MMM d", CultureInfo.InvariantCulture);
    public static string Date(DateTime date) => Date(DateOnly.FromDateTime(date));

    // "1 day", "3 days"
    public static string Days(int days) => Count(days, "day", "days");

    public static string Count(int count, string one, string many) => $"{count} {(count == 1 ? one : many)}";

    // "just now", "12 min ago", "3 h ago", "yesterday", "4 days ago", then "Nov 2".
    public static string Ago(DateTime at, DateTime now)
    {
        var span = now - at;
        if (span.TotalMinutes < 1)
            return "just now";
        if (span.TotalHours < 1)
            return $"{(int)span.TotalMinutes} min ago";
        if (at.Date == now.Date)
            return $"{(int)span.TotalHours} h ago";
        var days = (now.Date - at.Date).Days;
        return days switch
        {
            1 => "yesterday",
            < 7 => $"{days} days ago",
            _ => Date(at)
        };
    }

    // First letters of the first two words of the name, uppercase.
    public static string Initials(string fullName) =>
        string.Concat(fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0])));

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColor();

    // weight = the share of "with".
    private static string Mix(string color, string with, double weight)
    {
        static (int R, int G, int B) Channels(string hex) => (
            int.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber),
            int.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber),
            int.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber));
        var a = Channels(color);
        var b = Channels(with);
        int Blend(int x, int y) => (int)Math.Round(x * (1 - weight) + y * weight);
        return $"#{Blend(a.R, b.R):X2}{Blend(a.G, b.G):X2}{Blend(a.B, b.B):X2}";
    }
}
