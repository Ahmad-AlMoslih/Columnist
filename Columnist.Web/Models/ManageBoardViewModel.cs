using Columnist.Teams.Features.BoardAccess.Queries.GetBoardAccess.Models;
using Columnist.Teams.Features.Boards.Queries.GetBoardSettings.Models;

namespace Columnist.Web.Models;

// What the Manage board page shows: the Access section or the Columns section of one board (BoardToken null = new board).
public sealed record ManageBoardViewModel(string? BoardToken, string Title, string Section, GetBoardAccessModel? Access, GetBoardSettingsModel? Columns);
