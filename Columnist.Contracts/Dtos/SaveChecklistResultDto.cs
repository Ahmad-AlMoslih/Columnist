namespace Columnist.Contracts.Dtos
{
    // dbo.SaveChecklist / dbo.SaveChecklistItem: Result 0 = ok, 1 = not found.
    public sealed record SaveChecklistResultDto
    {
        public int Result { get; init; }
        public int? Id { get; init; }
    }
}
