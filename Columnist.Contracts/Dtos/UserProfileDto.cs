namespace Columnist.Contracts.Dtos;

// A row of dbo.GetUserProfile.
public sealed record UserProfileDto
{
    public string FullName { get; init; } = "";
    public string? Username { get; init; }
    public string Email { get; init; } = "";
    public string? PhotoName { get; init; }
    public int SessionVersion { get; init; }
}
