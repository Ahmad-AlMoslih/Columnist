using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Columnist.Web.Application.Components.Helpers;

public static class TaskCardHelpers
{
    public static IHtmlContent DueDateChip(this IHtmlHelper html, DateTime? dueDate, bool isDone = false)
    {
        var today = DateTime.Today;
        var (text, cssClass) = dueDate?.Date switch
        {
            var date when isDone && date < today => ($"Done (Late)", "kb-chip kb-chip-done"),
            _ when isDone => ("Done", "kb-chip kb-chip-done"),
            null => ("No date", "kb-chip kb-chip-muted"),
            var date when date < today => ($"Overdue · {date:MMM d}", "kb-chip kb-chip-overdue"),
            var date when date == today => ("Due today", "kb-chip kb-chip-soon"),
            var date when date == today.AddDays(1) => ("Due tomorrow", "kb-chip kb-chip-soon"),
            var date => ($"{date:MMM d}", "kb-chip")
        };

        var chip = new TagBuilder("span");
        chip.AddCssClass(cssClass);
        chip.InnerHtml.Append(text);
        return chip;
    }

    public static IHtmlContent Avatar(this IHtmlHelper html, string? fullName)
    {
        var person = new TagBuilder("span");
        person.AddCssClass("kb-person");

        var avatar = new TagBuilder("span");
        var name = new TagBuilder("span");
        name.AddCssClass("kb-person-name");

        var words = (fullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            avatar.AddCssClass("kb-avatar kb-avatar-none");
            avatar.InnerHtml.Append("?");
            name.AddCssClass("kb-person-name-none");
            name.InnerHtml.Append("Unassigned");
        }
        else
        {
            var colorIndex = fullName!.Sum(c => c) % 4;   // stable across restarts, unlike GetHashCode
            avatar.AddCssClass($"kb-avatar kb-avatar-{colorIndex}");
            avatar.InnerHtml.Append(string.Concat(words.Take(2).Select(w => char.ToUpperInvariant(w[0]))));
            name.InnerHtml.Append(words.Length > 1 ? $"{words[0]} {char.ToUpperInvariant(words[^1][0])}." : words[0]);
        }

        person.InnerHtml.AppendHtml(avatar);
        person.InnerHtml.AppendHtml(name);
        return person;
    }
}
