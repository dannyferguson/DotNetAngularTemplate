using MySqlConnector;

namespace DotNetAngularTemplate.Infrastructure.Services;

public class DatabaseUnitOfWork(MySqlConnection connection, MySqlTransaction transaction) : IAsyncDisposable
{
    private MySqlConnection Connection { get; } = connection;
    private MySqlTransaction Transaction { get; } = transaction;

    public async Task<int> ExecuteAsync(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        await using var cmd = new MySqlCommand(sql, Connection, Transaction);
        foreach (var (key, value) in parameters)
        {
            cmd.Parameters.AddWithValue(key, value);
        }

        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        await using var cmd = new MySqlCommand(sql, Connection, Transaction);
        foreach (var (key, value) in parameters)
        {
            cmd.Parameters.AddWithValue(key, value);
        }

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is DBNull or null ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default) => await Transaction.CommitAsync(cancellationToken);
    public async Task RollbackAsync(CancellationToken cancellationToken = default) => await Transaction.RollbackAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await Transaction.DisposeAsync();
        await Connection.DisposeAsync();
    }
}