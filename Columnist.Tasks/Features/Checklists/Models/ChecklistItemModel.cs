namespace Columnist.Tasks.Features.Checklists.Models;

public sealed record ChecklistItemModel
{
    public string Token { get; init; } = "";
    public string Text { get; init; } = "";
    public bool IsDone { get; init; }
}
