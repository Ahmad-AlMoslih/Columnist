namespace Columnist.Contracts.Dtos
{
    public sealed record TeamMemberDto
    {
        public int UserId { get; init; }
        public string FullName { get; init; } = "";
        public string? Username { get; init; }
        public bool IsOwner { get; init; }
        public DateTime JoinedAt { get; init; }
        public DateTime? AccessExpiresAt { get; init; }
        public int BoardCount { get; init; }
        public int TaskCount { get; init; }
    }
}
