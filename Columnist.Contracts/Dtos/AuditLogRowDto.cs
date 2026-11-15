namespace Columnist.Contracts.Dtos;

public sealed record AuditLogRowDto
{
    public long Id { get; init; }
    public string Action { get; init; } = "";
    public string Category { get; init; } = "";
    public int ActorId { get; init; }
    public string ActorName { get; init; } = "";
    public string? TargetName { get; init; }
    public string? Field { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public DateTime CreatedAt { get; init; }
    public int TotalCount { get; init; }
    public string? BoardName { get; init; }   // only dbo.GetTeamLogs returns it
}
