namespace Columnist.Contracts.Dtos
{
    public sealed record ColumnStatusDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public string? Value { get; init; }
    }
}
