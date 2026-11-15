using Columnist.Contracts.Dtos;
using Columnist.Services.Tokens.Extensions;

namespace Columnist.Tasks.Features.Checklists.Models;

public sealed record ChecklistModel
{
    public string Token { get; init; } = "";
    public string Title { get; init; } = "";
    public IReadOnlyList<ChecklistItemModel> Items { get; init; } = [];
    public int Done { get; init; }
    public int Total { get; init; }
    public bool IsAddingItem { get; init; }   // its "Add an item" box is shown open

    // dbo.GetTaskChecklists rows → one model per checklist, in the procedure's order.
    public static IReadOnlyList<ChecklistModel> FromRows(IEnumerable<TaskChecklistRowDto> rows, int? addingItemChecklistId = null) =>
        rows
            .GroupBy(r => r.ChecklistId)
            .Select(g =>
            {
                var items = g
                    .Where(r => r.ItemId is not null)
                    .Select(r => new ChecklistItemModel
                    {
                        Token = r.ItemId!.Value.Encrypt(),
                        Text = r.Text ?? "",
                        IsDone = r.IsDone == true
                    })
                    .ToList();
                return new ChecklistModel
                {
                    Token = g.Key.Encrypt(),
                    Title = g.First().Title,
                    Items = items,
                    Done = items.Count(i => i.IsDone),
                    Total = items.Count,
                    IsAddingItem = g.Key == addingItemChecklistId
                };
            })
            .ToList();
}
