namespace Columnist.Contracts.Dtos;

public sealed record BoardColumnDto
{
    public int BoardId { get; init; }
    public string BoardName { get; init; } = "";
    public int ColumnId { get; init; }
    public string ColumnName { get; init; } = "";
    public string StatusColor { get; init; } = "";
    public int Position { get; init; }
}
