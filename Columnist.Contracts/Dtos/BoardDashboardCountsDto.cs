namespace Columnist.Contracts.Dtos;

// dbo.GetBoardDashboardCounts: the number on each Dashboard card.
public sealed record BoardDashboardCountsDto
{
    public int Overdue { get; init; }
    public int Week { get; init; }
    public int Mine { get; init; }
    public int Unassigned { get; init; }
    public int Checklists { get; init; }
    public int Completed { get; init; }
    public int Created { get; init; }
    public int Activity { get; init; }
}
