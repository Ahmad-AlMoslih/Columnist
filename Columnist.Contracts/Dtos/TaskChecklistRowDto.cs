namespace Columnist.Contracts.Dtos
{
    // One row of dbo.GetTaskChecklists: a checklist item, or the checklist alone (ItemId NULL) when it has no items.
    public sealed record TaskChecklistRowDto
    {
        public int ChecklistId { get; init; }
        public string Title { get; init; } = "";
        public int ChecklistPosition { get; init; }
        public int? ItemId { get; init; }
        public string? Text { get; init; }
        public int? ItemPosition { get; init; }
        public bool? IsDone { get; init; }
    }
}
