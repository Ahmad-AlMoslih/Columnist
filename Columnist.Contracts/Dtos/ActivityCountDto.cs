namespace Columnist.Contracts.Dtos;

// A row of dbo.GetBoardActivityCounts: how many entries of one action a person made on a board since a time.
public sealed record ActivityCountDto
{
    public int UserId { get; init; }
    public string Action { get; init; } = "";
    public int Total { get; init; }
}
