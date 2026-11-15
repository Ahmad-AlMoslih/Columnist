namespace Columnist.Contracts.Dtos;

public sealed record LogActorDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
}
