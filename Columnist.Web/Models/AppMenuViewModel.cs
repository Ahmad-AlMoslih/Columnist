using Columnist.Teams.Features.Boards.Queries.GetMyBoards.Models;
using Columnist.Teams.Features.Teams.Queries.GetMyTeams.Models;

namespace Columnist.Web.Models;

// What the burger drawer shows.
public sealed record AppMenuViewModel(GetMyTeamsModel Teams, IReadOnlyList<MyBoardModel> Boards);
