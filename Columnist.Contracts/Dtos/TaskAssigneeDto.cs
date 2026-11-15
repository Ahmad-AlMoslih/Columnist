namespace Columnist.Contracts.Dtos;

public sealed record TaskAssigneeDto
{
    public int TaskId { get; init; }
    public int UserId { get; init; }
    public string FullName { get; init; } = "";
    public bool HasAccess { get; init; }
}
