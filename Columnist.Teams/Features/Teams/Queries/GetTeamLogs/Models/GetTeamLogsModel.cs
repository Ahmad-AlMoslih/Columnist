using Columnist.Contracts.Dtos;

namespace Columnist.Teams.Features.Teams.Queries.GetTeamLogs.Models;

public sealed record GetTeamLogsModel
{
    public IReadOnlyList<AuditEntryModel> Entries { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public string? Category { get; init; }
    public string? ActorToken { get; init; }
    public string? Search { get; init; }
    public IReadOnlyList<TokenOptionDto> Actors { get; init; } = [];
    public DateOnly? LastDay { get; init; }
}
