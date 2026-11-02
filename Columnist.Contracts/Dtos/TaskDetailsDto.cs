namespace Columnist.Contracts.Dtos;

public sealed record TaskDetailsDto
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public int ColumnId { get; init; }
    public int? AssigneeId { get; init; }
    public DateTime? DueDate { get; init; }
}