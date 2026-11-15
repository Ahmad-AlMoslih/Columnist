namespace Columnist.Contracts.Dtos
{
    public sealed record TeamBoardDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public int MemberCount { get; init; }
        public int ColumnCount { get; init; }
    }
}
