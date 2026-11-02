using Dapper;
using System.Data;

namespace Columnist.DataAccess;

public sealed class DbExecutor : IDbExecutor
{
    //(DIP)
    private readonly IDbConnectionFactory _connections;

    //hold connection factory
    public DbExecutor(IDbConnectionFactory connections) => _connections = connections;

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string procedure, DynamicParameters? parameters = null, CancellationToken ct = default)
    {
        //asks for a new connection
        using var connection = _connections.Create();
        //DApper Method, builds the desc then dapper runs the procedure and turns each row to t object
        var rows = await connection.QueryAsync<T>(Command(procedure, parameters, ct));
        return rows.ToList();
    }
    //dapper
    public async Task<T?> QuerySingleOrDefaultAsync<T>(string procedure, DynamicParameters? parameters = null, CancellationToken ct = default)
    {
        using var connection = _connections.Create();
        return await connection.QuerySingleOrDefaultAsync<T>(Command(procedure, parameters, ct));
    }
    //dapper
    public async Task<bool> ExecuteAsync(string procedure, DynamicParameters? parameters = null, CancellationToken ct = default)
    {
        parameters ??= new DynamicParameters();
        parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        using var connection = _connections.Create();
        await connection.ExecuteAsync(Command(procedure, parameters, ct));

        return parameters.Get<int>("@ReturnValue") == 0;
    }
    //helper
    private static CommandDefinition Command(string procedure, object? parameters, CancellationToken ct) =>
        new(procedure, parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
}