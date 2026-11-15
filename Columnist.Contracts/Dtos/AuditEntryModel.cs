namespace Columnist.Contracts.Dtos;

// One audit log entry as the pages show it (values already turned into labels).
public sealed record AuditEntryModel
{
    public string ActorName { get; init; } = "";
    public string ActorInitials { get; init; } = "";
    public string? PhotoUrl { get; init; }   // profile photo; null = show the initials
    public string Action { get; init; } = "";
    public string Category { get; init; } = "";
    public string? TargetName { get; init; }
    public string? Field { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? BoardName { get; init; }   // team logs only
}
