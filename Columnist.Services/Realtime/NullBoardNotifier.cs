namespace Columnist.Services.Realtime;

// Default: does nothing (used by the API). The Web app replaces it with the SignalR one.
public sealed class NullBoardNotifier : IBoardNotifier
{
    public Task BoardChangedAsync(int boardId, CancellationToken ct = default) => Task.CompletedTask;
    public Task CommentsChangedAsync(int boardId, CancellationToken ct = default) => Task.CompletedTask;
    public Task AccessChangedAsync(int userId, string message, CancellationToken ct = default) => Task.CompletedTask;
}
