namespace Columnist.Contracts.Dtos;

// A row of dbo.GetBoardTaskHistory: when a task came into its column and who moved it there, and its latest change.
// Without a TaskMoved entry: the task's creation (CreatedAt, CreatedBy). Without any audit entry: CreatedAt.
public sealed record ReportTaskHistoryDto
{
    public int TaskId { get; init; }
    public DateTime ColumnEnteredAt { get; init; }
    public string? ColumnEnteredByName { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}
