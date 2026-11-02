namespace Columnist.Contracts.Dtos;

public sealed class DeletedTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string BoardName { get; set; } = "";
    public string ColumnName { get; set; } = "";
    public DateTime DeletedAt { get; set; }
    public string? DeletedByName { get; set; }
}
