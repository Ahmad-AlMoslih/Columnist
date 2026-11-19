namespace Columnist.Contracts.Dtos;

// A row of dbo.GetImagesByTaskId.
public sealed record TaskImageDto
{
    public string FileName { get; init; } = "";
    public int Id { get; init; }
}
