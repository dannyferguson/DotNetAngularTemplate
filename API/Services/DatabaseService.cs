using DotNetAngularTemplate.Exceptions;
using MySqlConnector;

namespace DotNetAngularTemplate.Services;

public class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly string _connectionString;

    public DatabaseService(ILogger<DatabaseService> logger, string connectionString)
    {
        _logger = logger;
        _connectionString = connectionString;
    }

    private async Task<MySqlConnection> GetOpenConnectionAsync()
    {
        var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }

    public async Task InsertUserAsync(string email, string passwordHash)
    {
        await using var conn = await GetOpenConnectionAsync();
        await using var cmd = new MySqlCommand("INSERT INTO users (email, password_hash) VALUES (@email, @password)", conn);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@password", passwordHash);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            _logger.LogInformation("User '{@Email}' inserted successfully.", email);
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Error inserting user '{@Email}'. SQL State: {@ExSqlState}, Error Code: {@ExNumber}", email, ex.SqlState, ex.Number);

            if (ex.Number == 1062 && ex.SqlState == "23000")
            {
                throw new DuplicateEmailException($"Email '{email}' is already taken.", ex);
            }

            throw;
        }
        
    }

    public async Task<string?> GetPasswordHashByEmailAsync(string email)
    {
        await using var conn = await GetOpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT password_hash FROM users WHERE email = @email", conn);
        cmd.Parameters.AddWithValue("@email", email);

        return (string?) await cmd.ExecuteScalarAsync();
    }
    
    public async Task<(int Id, string PasswordHash)?> GetUserByEmailAsync(string email)
    {
        await using var conn = await GetOpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT id, password_hash FROM users WHERE email = @Email", conn);
        cmd.Parameters.AddWithValue("@Email", email);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }
        
        var id = reader.GetInt32(0);
        var hash = reader.GetString(1);
        return (id, hash);
    }
    
    public async Task InsertPasswordResetCodeAsync(int userId, string code)
    {
        await using var conn = await GetOpenConnectionAsync();
        await using var cmd = new MySqlCommand("INSERT INTO users_password_reset_codes (user_id, code) VALUES (@user_id, @code)", conn);
        cmd.Parameters.AddWithValue("@user_id", userId);
        cmd.Parameters.AddWithValue("@code", code);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            _logger.LogInformation("Password Reset Code inserted successfully.");
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Error inserting password reset code. SQL State: {@ExSqlState}, Error Code: {@ExNumber}", ex.SqlState, ex.Number);

            if (ex.Number == 1062 && ex.SqlState == "23000")
            {
                throw new DuplicateEmailException($"Password reset code '{code}' is already taken.", ex);
            }

            throw;
        }
        
    }
}