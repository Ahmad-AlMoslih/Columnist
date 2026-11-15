namespace Columnist.Contracts.Dtos
{
    public sealed record BoardSettingsRowDto
    {
        public int BoardId { get; init; }
        public int TeamId { get; init; }
        public string BoardName { get; init; } = "";
        public int? ColumnId { get; init; }
        public string? ColumnName { get; init; }
        public int? StatusId { get; init; }
        public int TaskCount { get; init; }
    }
}
