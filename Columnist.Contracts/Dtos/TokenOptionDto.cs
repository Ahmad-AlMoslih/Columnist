namespace Columnist.Contracts.Dtos;

public sealed record TokenOptionDto
{
    public string Token { get; init; } = "";
    public string Name { get; init; } = "";
    public bool Selected { get; init; }
}
