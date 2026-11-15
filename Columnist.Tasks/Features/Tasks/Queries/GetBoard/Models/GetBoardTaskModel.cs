using Columnist.Tasks.Features.Tasks.Models;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed record GetBoardTaskModel
{
    public string Token { get; init; } = "";
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string StatusName { get; init; } = "";
    public IReadOnlyList<TaskAssigneeModel> Assignees { get; init; } = [];
    public DateTime? DueDate { get; init; }
    public int ImageCount { get; init; }
    public int CommentCount { get; init; }
    public int ChecklistDone { get; init; }
    public int ChecklistTotal { get; init; }
    public string? FirstImageUrl { get; init; }
}
