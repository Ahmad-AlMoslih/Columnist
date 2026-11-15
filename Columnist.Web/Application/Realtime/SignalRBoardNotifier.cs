using Columnist.Services.Realtime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Columnist.Web.Application.Realtime;

public sealed class SignalRBoardNotifier : IBoardNotifier
{
    // The browser sends its SignalR connection id in this header, so it doesn't get its own change back.
    private const string ConnectionHeader = "X-Board-Connection";

    private readonly IHubContext<BoardHub> _hub;
    private readonly IHttpContextAccessor _http;

    public SignalRBoardNotifier(IHubContext<BoardHub> hub, IHttpContextAccessor http)
    {
        _hub = hub;
        _http = http;
    }

    public Task BoardChangedAsync(int boardId, CancellationToken ct = default) =>
        Others(boardId).SendAsync("BoardChanged", ct);

    public Task CommentsChangedAsync(int boardId, CancellationToken ct = default) =>
        Others(boardId).SendAsync("CommentsChanged", ct);

    // Only that user's browsers (SignalR matches the JWT's NameIdentifier claim).
    public Task AccessChangedAsync(int userId, string message, CancellationToken ct = default) =>
        _hub.Clients.User(userId.ToString()).SendAsync("AccessChanged", message, ct);

    // Viewers of that board only (BoardHub.JoinBoard), minus the sender.
    private IClientProxy Others(int boardId)
    {
        var group = BoardHub.BoardGroup(boardId);
        var connectionId = _http.HttpContext?.Request.Headers[ConnectionHeader].ToString();
        return string.IsNullOrEmpty(connectionId)
            ? _hub.Clients.Group(group)
            : _hub.Clients.GroupExcept(group, connectionId);
    }
}
