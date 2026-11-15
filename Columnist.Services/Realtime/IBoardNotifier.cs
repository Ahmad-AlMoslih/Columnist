namespace Columnist.Services.Realtime;

// Tells open browsers that the board or comments changed, so they refresh.
public interface IBoardNotifier
{
    Task BoardChangedAsync(int boardId, CancellationToken ct = default);
    Task CommentsChangedAsync(int boardId, CancellationToken ct = default);
    Task AccessChangedAsync(int userId, string message, CancellationToken ct = default);
}
