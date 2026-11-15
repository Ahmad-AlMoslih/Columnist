namespace Columnist.Contracts.Dtos
{
    public sealed record BoardAccessRowDto
    {
        public int UserId { get; init; }
        public string FullName { get; init; } = "";
        public string? Username { get; init; }
        public DateTime? AccessExpiresAt { get; init; }
        public int? PermissionId { get; init; }
        public int AssignedTaskCount { get; init; }
    }
}
