namespace Columnist.Contracts.Dtos
{
    public sealed record JoinTeamResultDto
    {
        public int Result { get; init; }
        public int? TeamId { get; init; }
    }
}
