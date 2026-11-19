using Columnist.Auth.Authorization;
using Columnist.Auth.Security.CurrentUser;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Dapper;

namespace Columnist.Auth.Security.Boards;

public sealed class BoardAccessService : IBoardAccessService
{
    private readonly IDbExecutor _db;
    private readonly IUserContext _userContext;

    // Per request: each board, column and task is read at most once.
    private readonly Dictionary<int, bool> _canView = new();
    private readonly Dictionary<int, bool> _isOwner = new();
    private readonly Dictionary<int, IReadOnlySet<string>> _permissions = new();
    private readonly Dictionary<int, int?> _boardByColumn = new();
    private readonly Dictionary<int, int?> _boardByTask = new();

    public BoardAccessService(IDbExecutor db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<IReadOnlyList<int>?> ResolveAsync(BoardScope scope, CancellationToken ct = default)
    {
        if (scope.IsEmpty)
            return null;

        var ids = new List<int>();

        if (!string.IsNullOrEmpty(scope.BoardToken))
        {
            var boardId = scope.BoardToken.Decrypt<int>();
            if (boardId is not null)
                ids.Add(boardId.Value);
        }

        if (!string.IsNullOrEmpty(scope.ColumnToken))
        {
            var columnId = scope.ColumnToken.Decrypt<int>();
            if (columnId is not null)
            {
                if (!_boardByColumn.TryGetValue(columnId.Value, out var boardId))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ColumnId", columnId.Value);

                    boardId = await _db.QuerySingleOrDefaultAsync<int?>("dbo.GetBoardIdByColumn", parameters, ct);
                    _boardByColumn[columnId.Value] = boardId;
                }
                if (boardId is not null)
                    ids.Add(boardId.Value);
            }
        }

        if (!string.IsNullOrEmpty(scope.TaskToken))
        {
            var taskId = scope.TaskToken.Decrypt<int>();
            if (taskId is not null)
            {
                if (!_boardByTask.TryGetValue(taskId.Value, out var boardId))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TaskId", taskId.Value);

                    boardId = await _db.QuerySingleOrDefaultAsync<int?>("dbo.GetBoardIdByTask", parameters, ct);
                    _boardByTask[taskId.Value] = boardId;
                }
                if (boardId is not null)
                    ids.Add(boardId.Value);
            }
        }

        return ids.Distinct().ToList();
    }

    public async Task<bool> CanViewAsync(int boardId, CancellationToken ct = default)
    {
        if (_userContext.UserId is not int userId)
            return false;
        if (_canView.TryGetValue(boardId, out var loaded))
            return loaded;

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@BoardId", boardId);

        var canView = await _db.QuerySingleOrDefaultAsync<bool?>("dbo.CanViewBoard", parameters, ct) ?? false;
        _canView[boardId] = canView;
        return canView;
    }

    public async Task<bool> IsOwnerAsync(int boardId, CancellationToken ct = default)
    {
        if (_userContext.UserId is not int userId)
            return false;
        if (_isOwner.TryGetValue(boardId, out var loaded))
            return loaded;

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@BoardId", boardId);

        var isOwner = await _db.QuerySingleOrDefaultAsync<bool?>("dbo.IsBoardOwner", parameters, ct) ?? false;
        _isOwner[boardId] = isOwner;
        return isOwner;
    }

    public async Task<bool> HasPermissionAsync(int boardId, Permission permission, CancellationToken ct = default)
    {
        if (_userContext.UserId is not int userId)
            return false;

        if (!_permissions.TryGetValue(boardId, out var names))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@BoardId", boardId);

            var rows = await _db.QueryAsync<string>("dbo.GetBoardPermissions", parameters, ct);
            names = new HashSet<string>(rows, StringComparer.OrdinalIgnoreCase);
            _permissions[boardId] = names;
        }

        return names.Contains(permission.ToString());
    }
}
