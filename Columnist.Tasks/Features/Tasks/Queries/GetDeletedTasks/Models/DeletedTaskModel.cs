namespace Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks.Models;

public sealed record DeletedTaskModel
{
    public string Token { get; init; } = "";
    public string Key { get; init; } = "";   // stable per task (TokenExtensions.ToStableKey): the row's id
    public string Title { get; init; } = "";
    public string BoardName { get; init; } = "";
    public string ColumnName { get; init; } = "";
    public DateTime DeletedAt { get; init; }
    public string? DeletedByName { get; init; }
    public IReadOnlyList<string> ImageUrls { get; init; } = [];
}
