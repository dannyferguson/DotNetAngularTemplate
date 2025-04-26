using DotNetAngularTemplate.Exceptions;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
using MySqlConnector;

namespace DotNetAngularTemplate.Services;

public class AuthService(ILogger<AuthService> logger, DatabaseService dbService)
{
    public async Task<Result> RegisterUserAsync(string email, string password)
    {
        var hashedPassword = PasswordHelper.HashPassword(password);

        try
        {
            await dbService.InsertUserAsync(email, hashedPassword);
            return Result.Success();
        }
        catch (DuplicateEmailException ex)
        {
            logger.LogWarning(ex, "Registration failed due to duplicate email: {@email}", email);
            return Result.Failure("Email address is already registered.");
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex, "Error during user registration for email: {@email}", email);
            return Result.Failure("An unexpected error occurred during registration.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during user registration for email: {@email}", email);
            return Result.Failure("An unexpected error occurred.");
        }
    }

    public async Task<bool> LoginUserAsync(string email, string password)
    {
        var storedHash = await dbService.GetPasswordHashByEmailAsync(email);
        if (storedHash == null)
        {
            return false;
        }

        return PasswordHelper.VerifyPassword(password, storedHash);
    }
    
    public async Task<int?> LoginUserAndGetIdAsync(string email, string password)
    {
        var user = await dbService.GetUserByEmailAsync(email);
        if (user == null)
            return null;

        var passwordValid = PasswordHelper.VerifyPassword(password, user.Value.PasswordHash);
        return passwordValid ? user.Value.Id : null;
    }
}