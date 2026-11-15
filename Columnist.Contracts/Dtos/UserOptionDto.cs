namespace Columnist.Contracts.Dtos;

// A row of dbo.GetUsersForDropdown.
public sealed record UserOptionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public bool HasAccess { get; init; }   // false = their team access ran out
}
