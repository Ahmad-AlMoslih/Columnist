namespace Columnist.Contracts.Dtos
{
    public sealed record InviteCodeDto
    {
        public string? InviteCode { get; init; }
        public DateTime? InviteCodeExpiresAt { get; init; }
    }
}
