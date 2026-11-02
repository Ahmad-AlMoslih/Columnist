namespace Columnist.Tasks.Features.Tasks.Queries.GetAllTasks;

public sealed record GetAllTasksModel
{
    public string Token { get; init; } = "";
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string BoardToken { get; init; } = "";
    public string BoardName { get; init; } = "";
    public string ColumnToken { get; init; } = "";
    public string ColumnName { get; init; } = "";
    public string? AssigneeName { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public string StatusColor { get; init; } = "";
    public string? FirstImageName { get; init; }
    public int ImageCount { get; init; }
    public string? FirstImageUrl { get; init; }
}
