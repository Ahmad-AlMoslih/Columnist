using Columnist.Auth.Security.Boards;
using Columnist.Services.Tokens.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Columnist.Web.Application.Realtime;

// Browsers connect here to receive "BoardChanged" / "CommentsChanged" for the board they are viewing.
[Authorize]
public sealed class BoardHub : Hub
{
    // Per connection: the board group it is currently in.
    private const string BoardGroupKey = "BoardGroup";

    private readonly IBoardAccessService _boards;

    public BoardHub(IBoardAccessService boards) => _boards = boards;

    public static string BoardGroup(int boardId) => $"board-{boardId}";

    public async Task JoinBoard(string boardToken)
    {
        var boardId = boardToken.Decrypt<int>();
        if (boardId is null || !await _boards.CanViewAsync(boardId.Value, Context.ConnectionAborted))
            return;

        await LeaveBoard();

        var group = BoardGroup(boardId.Value);
        await Groups.AddToGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
        Context.Items[BoardGroupKey] = group;
    }

    public async Task LeaveBoard()
    {
        if (Context.Items.TryGetValue(BoardGroupKey, out var previous) && previous is string group)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
            Context.Items.Remove(BoardGroupKey);
        }
    }
}
