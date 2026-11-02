namespace Columnist.Contracts.Dtos
{
    public sealed record TaskItemDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public int BoardId { get; init; }
        public string BoardName { get; init; } = "";
        public int ColumnId { get; init; }
        public string ColumnName { get; init; } = "";
        public string? AssigneeName { get; init; }
        public DateTime? DueDate { get; init; }
        public DateTime CreatedAt { get; init; }

        public string StatusColor { get; init; } = "";

        public string? FirstImageName { get; init; }
        public int ImageCount { get; init; }

    }
}

