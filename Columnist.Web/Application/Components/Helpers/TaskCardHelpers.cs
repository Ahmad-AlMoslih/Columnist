using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Columnist.Web.Application.Components.Helpers;

public static class TaskCardHelpers
{
    // forCard: the board card's smaller chip (no "No date" chip, "✓ Done", soon dates in plain grey).
    public static IHtmlContent DueDateChip(this IHtmlHelper html, DateTime? dueDate, bool isDone = false, bool forCard = false)
    {
        var today = DateTime.Today;
        if (forCard)
        {
            var (cardText, cardClass) = dueDate?.Date switch
            {
                var date when isDone && date < today => ("✓ Done (late)", "kb-card-due kb-card-due-done"),
                _ when isDone => ("✓ Done", "kb-card-due kb-card-due-done"),
                null => (null, null),
                var date when date < today => ($"Overdue · {date:MMM d}", "kb-card-due kb-card-due-overdue"),
                var date when date == today => ("Due today", "kb-card-due"),
                var date when date == today.AddDays(1) => ("Due tomorrow", "kb-card-due"),
                var date => ($"{date:MMM d}", "kb-card-due")
            };
            if (cardText is null)
                return HtmlString.Empty;

            var cardChip = new TagBuilder("span");
            cardChip.AddCssClass(cardClass!);
            cardChip.InnerHtml.Append(cardText);
            return cardChip;
        }

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

    // "?" avatar + "Unassigned".
    public static IHtmlContent Unassigned(this IHtmlHelper html)
    {
        var person = new TagBuilder("span");
        person.AddCssClass("kb-person");

        var avatar = new TagBuilder("span");
        avatar.AddCssClass("kb-avatar kb-avatar-none");
        avatar.InnerHtml.Append("?");

        var name = new TagBuilder("span");
        name.AddCssClass("kb-person-name kb-person-name-none");
        name.InnerHtml.Append("Unassigned");

        person.InnerHtml.AppendHtml(avatar);
        person.InnerHtml.AppendHtml(name);
        return person;
    }

    // Just the coloured initials circle (or the profile photo when there is one); greyed when the person has no access.
    public static IHtmlContent AvatarCircle(this IHtmlHelper html, string fullName, string? initials = null, bool noAccess = false, string? photoUrl = null)
    {
        var colorIndex = fullName.Sum(c => c) % 4;   // stable across restarts, unlike GetHashCode
        var avatar = new TagBuilder("span");
        avatar.AddCssClass($"kb-avatar kb-avatar-{colorIndex}");
        if (noAccess)
            avatar.AddCssClass("kb-avatar-noaccess");
        if (photoUrl is not null)
        {
            var photo = new TagBuilder("img");
            photo.TagRenderMode = TagRenderMode.SelfClosing;
            photo.AddCssClass("kb-photo");
            photo.Attributes["src"] = photoUrl;
            photo.Attributes["alt"] = "";
            avatar.InnerHtml.AppendHtml(photo);
        }
        else
        {
            avatar.InnerHtml.Append(initials ?? Initials(fullName));
        }
        return avatar;
    }

    // "Sara Hassan" → "Sara H."
    public static string ShortName(string fullName)
    {
        var words = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 1 ? $"{words[0]} {char.ToUpperInvariant(words[^1][0])}." : words.FirstOrDefault() ?? "";
    }

    private static string Initials(string fullName) =>
        string.Concat(fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0])));
}
