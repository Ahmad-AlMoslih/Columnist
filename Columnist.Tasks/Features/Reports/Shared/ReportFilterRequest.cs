namespace Columnist.Tasks.Features.Reports.Shared;

// The report filter's query string: ?q=..&assignee=..&from=..&to=..&column=..&column=..
// Raw values; ReportFilter ignores anything it can't read.
public sealed class ReportFilterRequest
{
    public string? Q { get; set; }           // title search, applies at SearchRules.MinLength characters
    public string? Assignee { get; set; }    // encrypted user token
    public string? From { get; set; }        // yyyy-MM-dd
    public string? To { get; set; }          // yyyy-MM-dd
    public string[]? Column { get; set; }    // encrypted column tokens
}
