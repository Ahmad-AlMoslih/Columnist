using Columnist.Contracts.Dtos;
using Columnist.Tasks.Features.Checklists.Models;
using Columnist.Tasks.Features.Tasks.Models;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskView.Models;

public sealed record GetTaskViewModel
{
    public string Token { get; init; } = "";
    public string Key { get; init; } = "";   // stable per task (TokenExtensions.ToStableKey)
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string BoardName { get; init; } = "";
    public string ColumnName { get; init; } = "";
    public string StatusColor { get; init; } = "";
    public string StatusName { get; init; } = "";
    public IReadOnlyList<TaskAssigneeModel> Assignees { get; init; } = [];
    public DateTime? DueDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedByName { get; init; }
    public IReadOnlyList<string> ImageUrls { get; init; } = [];
    public IReadOnlyList<ChecklistModel> Checklists { get; init; } = [];
    public IReadOnlyList<AuditEntryModel> History { get; init; } = [];
}
