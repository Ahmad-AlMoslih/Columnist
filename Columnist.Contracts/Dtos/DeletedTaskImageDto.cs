namespace Columnist.Contracts.Dtos;

public sealed class DeletedTaskImageDto
{
    public int TaskId { get; set; }
    public string FileName { get; set; } = "";
}
