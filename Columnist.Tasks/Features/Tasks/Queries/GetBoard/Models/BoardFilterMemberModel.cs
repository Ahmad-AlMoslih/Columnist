namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

// A person in the board filter's assignee list.
public sealed record BoardFilterMemberModel
{
    public string Token { get; init; } = "";
    public string Name { get; init; } = "";
    public string? PhotoUrl { get; init; }   // profile photo; null = show the initials
    public bool HasAccess { get; init; }   // false = their team access ran out
    public bool Selected { get; init; }

    public string FirstName => Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? Name;
}
