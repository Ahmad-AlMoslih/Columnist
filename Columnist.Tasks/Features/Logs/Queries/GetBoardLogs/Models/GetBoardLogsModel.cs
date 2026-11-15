using Columnist.Contracts.Dtos;

namespace Columnist.Tasks.Features.Logs.Queries.GetBoardLogs.Models;

public sealed record GetBoardLogsModel
{
    public string BoardToken { get; init; } = "";
    public string BoardName { get; init; } = "";
    public IReadOnlyList<AuditEntryModel> Entries { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public string? Category { get; init; }
    public string? ActorToken { get; init; }
    public string? Search { get; init; }
    public IReadOnlyList<TokenOptionDto> Actors { get; init; } = [];
    public DateOnly? LastDay { get; init; }
}
