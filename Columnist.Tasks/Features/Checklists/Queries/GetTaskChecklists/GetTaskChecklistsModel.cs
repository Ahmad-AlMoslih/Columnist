using Columnist.Tasks.Features.Checklists.Models;

namespace Columnist.Tasks.Features.Checklists.Queries.GetTaskChecklists;

public sealed record GetTaskChecklistsModel
{
    public string TaskToken { get; init; } = "";
    public IReadOnlyList<ChecklistModel> Checklists { get; init; } = [];
}
