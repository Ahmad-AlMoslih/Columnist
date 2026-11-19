namespace Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks.Models;

public sealed record GetDeletedTasksModel
{
    public IReadOnlyList<DeletedTaskModel> Tasks { get; init; } = [];
    public string? Search { get; init; }
    public string? BoardToken { get; init; }

    // What the user may do on the board.
    public bool CanSeeDeleted { get; init; }
    public bool CanSeeLogs { get; init; }
    public bool CanManage { get; init; }   // Super Admin of the board's team
    public bool CanRestore { get; init; }  // DeletedTasksWrite
}
