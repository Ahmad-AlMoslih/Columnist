using Columnist.Auth.Authorization;

namespace Columnist.Auth.Security.Boards;

public interface IBoardAccessService
{
    // The board ids the scope points to; null when the scope names no board. Tokens that point to nothing are skipped (the handler then returns 404).
    Task<IReadOnlyList<int>?> ResolveAsync(BoardScope scope, CancellationToken ct = default);
    Task<bool> CanViewAsync(int boardId, CancellationToken ct = default);
    // The user is the Super Admin (owner) of the board's team (dbo.IsBoardOwner).
    Task<bool> IsOwnerAsync(int boardId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(int boardId, Permission permission, CancellationToken ct = default);
}
