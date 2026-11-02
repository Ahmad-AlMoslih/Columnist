namespace Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks.Models;

public sealed record GetDeletedTasksModel
{
    public IReadOnlyList<DeletedTaskModel> Tasks { get; init; } = [];
    public string? Search { get; init; }
}
