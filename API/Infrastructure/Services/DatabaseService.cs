using System.Data;
using DotNetAngularTemplate.Infrastructure.Models;
using MySqlConnector;

namespace DotNetAngularTemplate.Infrastructure.Services;

public record User(int Id, string Email, bool EmailVerified, string PasswordHash, DateTime CreatedAt, DateTime UpdatedAt);

public class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly string _connectionString;

    public DatabaseService(ILogger<DatabaseService> logger, string connectionString)
    {
        _logger = logger;
        _connectionString = connectionString;
    }
    
    public async Task<DatabaseUnitOfWork> BeginUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        return new DatabaseUnitOfWork(connection, transaction);
    }
    
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = await GetOpenConnectionAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to connect to MySQL on startup.");
            throw;
        }
    }

    private async Task<MySqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        return conn;
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, Dictionary<string, object> parameters,
        Func<IDataReader, T> map, CancellationToken cancellationToken = default)
    {
        await using var conn = await GetOpenConnectionAsync(cancellationToken);
        await using var cmd = new MySqlCommand(sql, conn);

        AddParameters(cmd, parameters);

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? map(reader) : default;
    }

    public async Task<List<T>> QueryManyAsync<T>(
        string sql,
        Dictionary<string, object> parameters,
        Func<IDataReader, T> map, CancellationToken cancellationToken = default)
    {
        await using var conn = await GetOpenConnectionAsync(cancellationToken);
        await using var cmd = new MySqlCommand(sql, conn);

        AddParameters(cmd, parameters);

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);

        var results = new List<T>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(map(reader));
        }

        return results;
    }

    public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, email_verified, password_hash, created_at, updated_at FROM users WHERE email = @Email";
        var parameters = new Dictionary<string, object>
        {
            ["@Email"] = email
        };

        try
        {
            var user = await QuerySingleAsync<User>(sql, parameters, reader =>
            {
                var id = reader.GetInt32(0);
                var emailVerified = reader.GetBoolean(1);
                var hash = reader.GetString(2);
                var createdAt = reader.GetDateTime(3);
                var updatedAt = reader.GetDateTime(4);
                return new User(id, email, emailVerified, hash, createdAt, updatedAt);
            }, cancellationToken);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while trying to retrieve user by email {Email} from the database",
                email);
            return null;
        }
    }

    public async Task<User?> GetUserById(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT email, email_verified, password_hash, created_at, updated_at FROM users WHERE id = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = id
        };

        try
        {
            var user = await QuerySingleAsync<User>(sql, parameters, reader =>
            {
                var email = reader.GetString(0);
                var emailVerified = reader.GetBoolean(1);
                var hash = reader.GetString(2);
                var createdAt = reader.GetDateTime(3);
                var updatedAt = reader.GetDateTime(4);
                return new User(id, email, emailVerified, hash, createdAt, updatedAt);
            }, cancellationToken);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while trying to retrieve user by id {Id} from the database", id);
            return null;
        }
    }

    public async Task<ApiResult> UpdateUserPassword(DatabaseUnitOfWork unitOfWork, int userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = "UPDATE users SET password_hash = @Password, updated_at = @UtcNow WHERE id = @UserId";
            var parameters = new Dictionary<string, object>
            {
                ["@Password"] = passwordHash,
                ["@UtcNow"] = DateTime.UtcNow,
                ["@UserId"] = userId,
            };
            await unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);
            _logger.LogInformation("User by id '{Id}' has successfully updated their password.", userId);
            return ApiResult.Success();
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex,
                "Error updating password for user id {UserId}. SQL State: {ExSqlState}, Error Code: {ExNumber}", userId,
                ex.SqlState, ex.Number);
            return ApiResult.Failure();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating password for user of id: {UserId}", userId);
            return ApiResult.Failure();
        }
    }

    private static void AddParameters(MySqlCommand cmd, Dictionary<string, object> parameters)
    {
        foreach (var (key, value) in parameters)
        {
            cmd.Parameters.AddWithValue(key, value);
        }
    }
}