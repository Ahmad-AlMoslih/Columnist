namespace Columnist.Contracts.Dtos
{
    public sealed record BoardCandidateDto
    {
        public int UserId { get; init; }
        public string FullName { get; init; } = "";
        public string? Username { get; init; }
        public DateTime? AccessExpiresAt { get; init; }
    }
}
