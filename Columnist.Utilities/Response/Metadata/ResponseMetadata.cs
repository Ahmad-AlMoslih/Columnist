namespace Columnist.Utilities.Response.Metadata
{
    public sealed record ResponseMetadata(
        DateTime Timestamp, string ApiVersion, PaginationMetadata? Pagination = null);
}
