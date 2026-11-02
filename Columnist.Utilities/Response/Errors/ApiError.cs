using Columnist.Utilities.Response.Enums;

namespace Columnist.Utilities.Response.Errors
{
    public sealed record ApiError
    {
        public ErrorClassification Classification { get; init; }
        public string? Field { get; init; }   // null when the error is not about one field
        public required string Message { get; init; }
        public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }
    }
}
