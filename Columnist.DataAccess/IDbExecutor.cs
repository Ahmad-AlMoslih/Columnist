using Dapper;

namespace Columnist.DataAccess
{
    public interface IDbExecutor
    {
        Task<IReadOnlyList<T>> QueryAsync<T>(string procedure, DynamicParameters? parameters = null, CancellationToken ct = default);
        Task<T?> QuerySingleOrDefaultAsync<T>(string procedure, DynamicParameters? parameters = null, CancellationToken ct = default);
        Task<bool> ExecuteAsync(string procedure, DynamicParameters? parameters = null, CancellationToken ct = default);
    }
}

