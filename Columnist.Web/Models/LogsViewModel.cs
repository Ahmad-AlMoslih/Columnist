using Columnist.Tasks.Features.Logs.Queries.GetBoardLogs.Models;

namespace Columnist.Web.Models;

// What the Logs page shows: the logs of one board.
public sealed record LogsViewModel(string Board, GetBoardLogsModel BoardLogs);
