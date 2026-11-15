using Columnist.Contracts.Dtos;
using Columnist.Services.Tokens.Extensions;

namespace Columnist.Tasks.Features.Tasks.Models;

public sealed record TaskAssigneeModel
{
    public string Token { get; init; } = "";
    public string FullName { get; init; } = "";
    public string Initials { get; init; } = "";
    public string? PhotoUrl { get; init; }   // profile photo; null = show the initials
    public string ShortName { get; init; } = "";   // "Sara H."
    public bool HasAccess { get; init; }

    public static TaskAssigneeModel From(TaskAssigneeDto row)
    {
        var words = row.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new TaskAssigneeModel
        {
            Token = row.UserId.Encrypt(),
            FullName = row.FullName,
            Initials = string.Concat(words.Take(2).Select(w => char.ToUpperInvariant(w[0]))),
            ShortName = words.Length > 1 ? $"{words[0]} {char.ToUpperInvariant(words[^1][0])}." : words.FirstOrDefault() ?? "",
            HasAccess = row.HasAccess
        };
    }
}
