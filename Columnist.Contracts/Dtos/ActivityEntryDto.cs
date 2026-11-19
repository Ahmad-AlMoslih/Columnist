namespace Columnist.Contracts.Dtos;

// A row of dbo.GetBoardRecentActivity: an audit entry of a board, with its task when that task still exists.
public sealed record ActivityEntryDto
{
    public string Action { get; init; } = "";
    public int ActorId { get; init; }
    public string ActorName { get; init; } = "";
    public string? TargetName { get; init; }
    public string? Field { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public DateTime CreatedAt { get; init; }
    public int? TaskId { get; init; }   // null = not a task entry, or the task was deleted
}
