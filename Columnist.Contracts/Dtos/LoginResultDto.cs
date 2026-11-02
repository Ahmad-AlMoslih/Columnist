namespace Columnist.Contracts.Dtos;

public sealed record LoginResultDto
{
    public string Token { get; init; } = "";
    public string RefreshToken { get; init; } = "";
    public DateTime RefreshTokenExpiresAt { get; init; }
    public string FullName { get; init; } = "";
    public string Role { get; init; } = "";
}
