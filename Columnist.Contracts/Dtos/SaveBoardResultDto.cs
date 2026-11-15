namespace Columnist.Contracts.Dtos
{
    public sealed record SaveBoardResultDto
    {
        public int Result { get; init; }
        public int? Id { get; init; }
    }
}
