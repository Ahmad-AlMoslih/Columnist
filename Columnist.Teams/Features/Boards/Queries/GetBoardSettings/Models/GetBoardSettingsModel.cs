namespace Columnist.Teams.Features.Boards.Queries.GetBoardSettings.Models;

public sealed record GetBoardSettingsModel
{
    public string? BoardToken { get; init; }
    public string TeamToken { get; init; } = "";
    public string Name { get; init; } = "";
    public bool IsNew { get; init; }
    public IReadOnlyList<BoardColumnSettingsModel> Columns { get; init; } = [];
    public IReadOnlyList<ColumnStatusModel> Statuses { get; init; } = [];
    // The board menu's links (existing board only).
    public bool CanSeeDeleted { get; init; }
    public bool CanSeeLogs { get; init; }
    public bool CanManage { get; init; }
}
