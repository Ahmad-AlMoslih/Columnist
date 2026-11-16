using Columnist.Tasks.Features.Checklists.Models;

namespace Columnist.Tasks.Features.Checklists.Queries.GetTaskChecklists;

public sealed record GetTaskChecklistsModel
{
    public string TaskToken { get; init; } = "";
    public string TaskKey { get; init; } = "";   // stable per task: remembers the checklists' open/closed state in the browser
    public IReadOnlyList<ChecklistModel> Checklists { get; init; } = [];
}
