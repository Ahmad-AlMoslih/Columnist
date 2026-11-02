namespace Columnist.Contracts.Dtos;

public sealed record UserCredentialsDto
{
    public int Id { get; init; }
    public string Username { get; init; } = "";
    public string FullName { get; init; } = "";
    public string? PasswordHash { get; init; }
    public string RoleName { get; init; } = "";
}
