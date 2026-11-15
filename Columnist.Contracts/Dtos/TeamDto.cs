namespace Columnist.Contracts.Dtos
{
    public sealed record TeamDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public bool IsOwner { get; init; }
        public DateTime? AccessExpiresAt { get; init; }
    }
}
