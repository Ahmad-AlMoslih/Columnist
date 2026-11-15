namespace Columnist.Contracts.Dtos
{
    public sealed record BoardChecklistCountDto
    {
        public int TaskId { get; init; }
        public int Total { get; init; }
        public int Done { get; init; }
    }
}
