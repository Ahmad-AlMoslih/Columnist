namespace Columnist.Contracts.Dtos;

// A person in the task form's assignee picker.
public sealed record AssigneeOptionDto
{
    public string Token { get; init; } = "";
    public string Name { get; init; } = "";
    public string? PhotoUrl { get; init; }   // profile photo; null = show the initials
    public bool Selected { get; init; }
    public bool HasAccess { get; init; }
}
