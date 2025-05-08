using System.Data;
using MySqlConnector;

namespace DotNetAngularTemplate.Services;

public record User(int Id, string Email, string PasswordHash, DateTime CreatedAt, DateTime UpdatedAt);

public class DatabaseService(Func<Task<MySqlConnection>> getConnectionFunc, ILogger<DatabaseService> logger)
{
    public async Task<int> ExecuteAsync(string sql, Dictionary<string, object> parameters)
    {
        await using var conn = await getConnectionFunc();
        await using var cmd = new MySqlCommand(sql, conn);
        foreach (var (key, value) in parameters)
        {
            cmd.Parameters.AddWithValue(key, value);
        }

        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters)
    {
        await using var conn = await getConnectionFunc();
        await using var cmd = new MySqlCommand(sql, conn);
        foreach (var (key, value) in parameters)
        {
            cmd.Parameters.AddWithValue(key, value);
        }

        return (T?) await cmd.ExecuteScalarAsync();
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, Dictionary<string, object> parameters, Func<IDataReader, T> map)
    {
        await using var conn = await getConnectionFunc();
        await using var cmd = new MySqlCommand(sql, conn);
        foreach (var (key, value) in parameters)
        {
            cmd.Parameters.AddWithValue(key, value);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? map(reader) : default;
    }
    
    public async Task<List<T>> QueryManyAsync<T>(
        string sql,
        Dictionary<string, object> parameters,
        Func<IDataReader, T> map)
    {
        await using var conn = await getConnectionFunc();
        await using var cmd = new MySqlCommand(sql, conn);
        foreach (var (key, value) in parameters)
        {
            cmd.Parameters.AddWithValue(key, value);
        }

        await using var reader = await cmd.ExecuteReaderAsync();

        var results = new List<T>();
        while (await reader.ReadAsync())
        {
            results.Add(map(reader));
        }

        return results;
    }
    
    public async Task<User?> GetUserByEmail(string email)
    {
        const string sql = "SELECT id, password_hash, created_at, updated_at FROM users WHERE email = @Email";
        var parameters = new Dictionary<string, object>
        {
            ["@Email"] = email
        };

        try
        {
            var user = await QuerySingleAsync<User>(sql, parameters, reader =>
            {
                var id = reader.GetInt32(0);
                var hash = reader.GetString(1);
                var createdAt = reader.GetDateTime(2);
                var updatedAt = reader.GetDateTime(3);
                return new User(id, email, hash, createdAt, updatedAt);
            });

            return user;
        } 
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occured while trying to retrieve user by email {Email} from the database", email);
            return null;
        }
    }
    
    public async Task<User?> GetUserById(int id)
    {
        const string sql = "SELECT email, password_hash, created_at, updated_at FROM users WHERE id = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = id
        };

        try
        {
            var user = await QuerySingleAsync<User>(sql, parameters, reader =>
            {
                var email = reader.GetString(0);
                var hash = reader.GetString(1);
                var createdAt = reader.GetDateTime(2);
                var updatedAt = reader.GetDateTime(3);
                return new User(id, email, hash, createdAt, updatedAt);
            });

            return user;
        } 
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occured while trying to retrieve user by id {Id} from the database", id);
            return null;
        }
    }
    
    
    // private readonly ILogger<DatabaseService> _logger;
    // private readonly string _connectionString;
    //
    // public DatabaseService(ILogger<DatabaseService> logger, string connectionString)
    // {
    //     _logger = logger;
    //     _connectionString = connectionString;
    // }
    //
    // private async Task<MySqlConnection> GetOpenConnectionAsync()
    // {
    //     var conn = new MySqlConnection(_connectionString);
    //     await conn.OpenAsync();
    //     return conn;
    // }
    
    //
    // public async Task<string?> GetPasswordHashByEmailAsync(string email)
    // {
    //     await using var conn = await GetOpenConnectionAsync();
    //     await using var cmd = new MySqlCommand("SELECT password_hash FROM users WHERE email = @Email", conn);
    //     cmd.Parameters.AddWithValue("@Email", email);
    //
    //     return (string?) await cmd.ExecuteScalarAsync();
    // }
    //
    // public async Task<string?> GetEmailByUserId(int userId)
    // {
    //     await using var conn = await GetOpenConnectionAsync();
    //     await using var cmd = new MySqlCommand("SELECT email FROM users WHERE id = @UserId", conn);
    //     cmd.Parameters.AddWithValue("@UserId", userId);
    //
    //     return (string?) await cmd.ExecuteScalarAsync();
    // }
    //
    // public async Task<(int Id, string PasswordHash)?> GetUserByEmailAsync(string email)
    // {
    //     await using var conn = await GetOpenConnectionAsync();
    //     await using var cmd = new MySqlCommand("SELECT id, password_hash FROM users WHERE email = @Email", conn);
    //     cmd.Parameters.AddWithValue("@Email", email);
    //
    //     await using var reader = await cmd.ExecuteReaderAsync();
    //
    //     if (!await reader.ReadAsync())
    //     {
    //         return null;
    //     }
    //     
    //     var id = reader.GetInt32(0);
    //     var hash = reader.GetString(1);
    //     return (id, hash);
    // }
    //
    // public async Task<int?> GetUserIdByForgotPasswordCode(string code)
    // {
    //     await using var conn = await GetOpenConnectionAsync();
    //     await using var cmd = new MySqlCommand("SELECT user_id FROM users_password_reset_codes WHERE code = @Code AND expires_at > @UtcNow", conn);
    //     cmd.Parameters.AddWithValue("@Code", code);
    //     cmd.Parameters.AddWithValue("@UtcNow", DateTime.UtcNow);
    //
    //     await using var reader = await cmd.ExecuteReaderAsync();
    //
    //     if (!await reader.ReadAsync())
    //     {
    //         return null;
    //     }
    //     
    //     var id = reader.GetInt32(0);
    //     return id;
    // }
    //
    // public async Task UpdateUserPassword(int userId, string passwordHash)
    // {
    //     await using var conn = await GetOpenConnectionAsync();
    //     await using var cmd = new MySqlCommand(
    //         "UPDATE users SET password_hash = @Password, updated_at = NOW() WHERE id = @UserId", conn);
    //
    //     cmd.Parameters.AddWithValue("@UserId", userId);
    //     cmd.Parameters.AddWithValue("@Password", passwordHash);
    //
    //     try
    //     {
    //         await cmd.ExecuteNonQueryAsync();
    //     }
    //     catch (MySqlException ex)
    //     {
    //         _logger.LogError(ex, "Error updating password for user id {UserId}. SQL State: {@ExSqlState}, Error Code: {@ExNumber}", userId, ex.SqlState, ex.Number);
    //         throw;
    //     }
    // }
}