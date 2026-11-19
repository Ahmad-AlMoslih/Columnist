using Columnist.Contracts.Dtos;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Models;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;

public sealed record GetBoardTaskModel
{
    public string Token { get; init; } = "";
    public string Key { get; init; } = "";   // stable per task (TokenExtensions.ToStableKey): the card's id
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
    public bool CanWrite { get; init; }
    public bool CanDelete { get; init; }

    // A board card (the board, and one card re-rendered after a drag into or out of a Done column).
    public static GetBoardTaskModel From(TaskItemDto task, IReadOnlyList<TaskAssigneeModel> assignees, BoardChecklistCountDto? checklists,
        string? firstImageUrl, bool canWrite, bool canDelete) => new()
    {
        Token = task.Id.Encrypt(),
        Key = task.Id.ToStableKey("task"),
        Title = task.Title,
        Description = task.Description,
        StatusName = task.StatusName,
        Assignees = assignees,
        DueDate = task.DueDate,
        ImageCount = task.ImageCount,
        CommentCount = task.CommentCount,
        ChecklistDone = checklists?.Done ?? 0,
        ChecklistTotal = checklists?.Total ?? 0,
        FirstImageUrl = firstImageUrl,
        CanWrite = canWrite,
        CanDelete = canDelete
    };
}
