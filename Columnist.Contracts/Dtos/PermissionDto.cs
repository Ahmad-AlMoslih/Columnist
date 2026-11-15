namespace Columnist.Contracts.Dtos
{
    public sealed class PermissionDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public string? Description { get; init; }
        public int GroupId { get; init; }
        public string GroupName { get; init; } = "";
        public string FunctionalityName { get; init; } = "";
    }
}
