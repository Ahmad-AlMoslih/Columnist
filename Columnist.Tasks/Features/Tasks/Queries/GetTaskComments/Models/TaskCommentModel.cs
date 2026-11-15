namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskComments.Models;

public sealed record TaskCommentModel
{
    public string Token { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public string AuthorInitials { get; init; } = "";
    public string? PhotoUrl { get; init; }   // profile photo; null = show the initials
    public string Body { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public bool IsEdited { get; init; }
    public bool IsMine { get; init; }
    public bool IsEditing { get; init; }
}
