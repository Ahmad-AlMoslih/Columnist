using Columnist.Contracts.Dtos;
using Columnist.Tasks.Features.Checklists.Models;
using Columnist.Tasks.Features.Tasks.Models;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskView.Models;

public sealed record GetTaskViewModel
{
    public string Token { get; init; } = "";   // empty for a draft (not created yet)
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
    public IReadOnlyList<TaskImageModel> Images { get; init; } = [];
    public IReadOnlyList<ChecklistModel> Checklists { get; init; } = [];
    public IReadOnlyList<AuditEntryModel> History { get; init; } = [];
    // What the user may do on the task's board.
    public bool CanWrite { get; init; }
    public bool CanDelete { get; init; }

    // In-place editing (only filled when CanWrite).
    public string? RowVersion { get; init; }   // Base64 of Tasks.RowVersion, sent back with every save
    public string BoardToken { get; init; } = "";
    public string ColumnToken { get; init; } = "";
    public IReadOnlyList<TaskColumnOptionModel> Columns { get; init; } = [];   // the column pill's list
    public IReadOnlyList<TaskAssigneeModel> Members { get; init; } = [];   // the board's people, current assignees included (Key matches Assignees[].Key)
    public bool IsDraft { get; init; }   // "New task": only the title and the column can be set

    // The image upload's limits (DmsOptions), for the upload preview's client checks.
    public int UploadMaxFiles { get; init; }
    public long UploadMaxBytes { get; init; }
    public IReadOnlyList<string> UploadAllowedTypes { get; init; } = [];   // file extensions, e.g. ".png"
}

public sealed record TaskImageModel(string Token, string Key, string Url);   // Token = encrypted TaskImages.Id; Key = stable per image (element ids)

public sealed record TaskColumnOptionModel(string Token, string Name, string StatusName, string StatusColor, bool Selected);
