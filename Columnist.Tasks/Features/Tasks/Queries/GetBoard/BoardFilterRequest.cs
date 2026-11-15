namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard;

// The board filter's query string: ?assignees=..&unassigned=1&due=..&from=..&to=..&status=..&unfinished=1.
// Raw values; GetBoardHandler ignores anything it can't read.
public sealed class BoardFilterRequest
{
    public string[]? Assignees { get; set; }   // encrypted user tokens
    public string? Unassigned { get; set; }    // "1"
    public string? Due { get; set; }           // overdue | today | week | none | custom
    public string? From { get; set; }          // yyyy-MM-dd, only with due=custom
    public string? To { get; set; }            // yyyy-MM-dd, only with due=custom
    public string[]? Status { get; set; }      // ToDo | InProgress | Done
    public string? Unfinished { get; set; }    // "1"
}
