using DotNetAngularTemplate.Exceptions;
using DotNetAngularTemplate.Models;
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
        await using var cmd = new MySqlCommand("INSERT INTO users (email, password_hash) VALUES (@Email, @Password)", conn);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@Password", passwordHash);

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
        await using var cmd = new MySqlCommand("SELECT password_hash FROM users WHERE email = @Email", conn);
        cmd.Parameters.AddWithValue("@Email", email);

        return (string?) await cmd.ExecuteScalarAsync();
    }
    
    public async Task<string?> GetEmailByUserId(int userId)
    {
        await using var conn = await GetOpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT email FROM users WHERE id = @UserId", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

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
    
    public async Task<int?> GetUserIdByForgotPasswordCode(string code)
    {
        await using var conn = await GetOpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT user_id FROM users_password_reset_codes WHERE code = @Code AND expires_at > @UtcNow", conn);
        cmd.Parameters.AddWithValue("@Code", code);
        cmd.Parameters.AddWithValue("@UtcNow", DateTime.UtcNow);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }
        
        var id = reader.GetInt32(0);
        return id;
    }
    
    public async Task UpdateUserPassword(int userId, string passwordHash)
    {
        await using var conn = await GetOpenConnectionAsync();
        await using var cmd = new MySqlCommand(
            "UPDATE users SET password_hash = @Password, updated_at = NOW() WHERE id = @UserId", conn);

        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Password", passwordHash);

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Error updating password for user id {UserId}. SQL State: {@ExSqlState}, Error Code: {@ExNumber}", userId, ex.SqlState, ex.Number);
            throw;
        }
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