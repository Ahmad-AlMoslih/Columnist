using Columnist.Tasks.Features.Tasks.Models;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdModel
{
    public string Token { get; init; } = "";
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string ColumnToken { get; init; } = "";
    public IReadOnlyList<TaskAssigneeModel> Assignees { get; init; } = [];
    public DateTime? DueDate { get; init; }
}
