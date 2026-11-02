namespace Columnist.Utilities.Response.Metadata
{
   public sealed record PaginationMetadata(int PageNumber, int PageSize, int TotalCount, int TotalPages);
}
