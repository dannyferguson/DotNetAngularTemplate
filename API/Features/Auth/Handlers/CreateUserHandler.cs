using DotNetAngularTemplate.Exceptions;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
using DotNetAngularTemplate.Models.Responses;
using DotNetAngularTemplate.Services;
using MySqlConnector;

namespace DotNetAngularTemplate.Features.Auth.Handlers;

public record CreateUser(string Email, string Password);

public class CreateUserHandler(ILogger<CreateUserHandler> logger, DatabaseService databaseService)
{
    public async Task<Result> Handle(CreateUser message)
    {
        var passwordHash = PasswordHelper.HashPassword(message.Password);
        
        const string sql = "INSERT INTO users (email, password_hash) VALUES (@Email, @Password)";
        var parameters = new Dictionary<string, object>
        {
            ["@Email"] = message.Email,
            ["@Password"] = passwordHash
        };

        try
        {
            await databaseService.ExecuteAsync(sql, parameters);
            logger.LogInformation("User '{@Email}' inserted successfully.", message.Email);
            // todo send registration email
            return Result.Success();
        }
        catch (DuplicateEmailException)
        {
            logger.LogInformation("Registration failed due to duplicate email: {@email}", message.Email);
            // We don't want to let users know a duplicate email was found for security reasons.
            return Result.Success(); 
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex, "Error during user registration for email: {@email}", message.Email);
            return Result.Failure("An unexpected error occurred.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during user registration for email: {@email}", message.Email);
            return Result.Failure("An unexpected error occurred.");
        }
    }
}