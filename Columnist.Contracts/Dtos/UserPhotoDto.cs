namespace Columnist.Contracts.Dtos;

// A row of dbo.GetUserPhotos.
public sealed record UserPhotoDto
{
    public int UserId { get; init; }
    public string PhotoName { get; init; } = "";
}
