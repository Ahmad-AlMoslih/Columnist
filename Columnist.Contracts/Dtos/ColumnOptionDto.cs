namespace Columnist.Contracts.Dtos;

// A row of dbo.GetBoardColumnOptions: a board's column with its status.
public sealed record ColumnOptionDto
{
    public int ColumnId { get; init; }
    public string ColumnName { get; init; } = "";
    public string StatusName { get; init; } = "";
    public string StatusColor { get; init; } = "";
    public string BoardName { get; init; } = "";
}
