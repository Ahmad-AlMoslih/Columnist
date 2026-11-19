using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Tasks.Features.Tasks.Commands;

// Shared by the task view's one-field saves (UpdateTaskTitle, UpdateTaskDescription, UpdateTaskDueDate, SetTaskAssignees, AddTaskImages, RemoveTaskImage).
internal static class TaskFieldSave
{
    public const int NotFoundResult = 1;
    public const int AssigneeNotFoundResult = 3;
    public const int ChangedByOtherResult = 4;
    public const int ImageNotFoundResult = 5;

    // Base64 RowVersion from the view; null = don't check. A malformed value can't match any version, so it is a conflict.
    public static bool TryParseRowVersion(string? value, out byte[]? rowVersion)
    {
        rowVersion = null;
        if (string.IsNullOrEmpty(value))
            return true;
        try
        {
            rowVersion = Convert.FromBase64String(value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    // A procedure's non-zero return value as the response.
    public static ApiResponse<bool> Failure(int result, string? field = null) => result switch
    {
        ChangedByOtherResult => ApiResponse<bool>.Conflict(ErrorMessages.TaskChangedByOther),
        AssigneeNotFoundResult => ApiResponse<bool>.BadRequest(ErrorMessages.AssigneeNotFound, field),
        ImageNotFoundResult => ApiResponse<bool>.NotFound(ErrorMessages.ImageNotFound),
        _ => ApiResponse<bool>.NotFound(ErrorMessages.TaskNotFound)
    };
}
