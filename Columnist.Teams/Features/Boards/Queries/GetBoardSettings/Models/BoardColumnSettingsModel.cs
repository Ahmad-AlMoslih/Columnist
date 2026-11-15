namespace Columnist.Teams.Features.Boards.Queries.GetBoardSettings.Models;

public sealed record BoardColumnSettingsModel
{
    public string? Token { get; init; }
    public string Name { get; init; } = "";
    public int StatusId { get; init; }
    public int TaskCount { get; init; }
}
