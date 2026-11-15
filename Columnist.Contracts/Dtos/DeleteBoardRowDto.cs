namespace Columnist.Contracts.Dtos
{
    public sealed record DeleteBoardRowDto
    {
        public int Result { get; init; }
        public string? FileName { get; init; }
    }
}
