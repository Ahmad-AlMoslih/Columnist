namespace Columnist.Contracts.Dtos
{
    public sealed record TaskCommentDto
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public string AuthorName { get; init; } = "";
        public string Body { get; init; } = "";
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
