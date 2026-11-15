namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskComments.Models;

public sealed record GetTaskCommentsModel
{
    public string TaskToken { get; init; } = "";
    public IReadOnlyList<TaskCommentModel> Comments { get; init; } = [];
}
