using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
using DotNetAngularTemplate.Services;
using MySqlConnector;

namespace DotNetAngularTemplate.Features.Auth.Register;

public class CreateUserCommandHandler(ILogger<CreateUserCommandHandler> logger, DatabaseService databaseService)
{
    public async Task<ApiResult> Handle(CreateUserCommand message)
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
            logger.LogInformation("User '{Email}' inserted successfully.", message.Email);
            // todo send registration email (email verification)
            return ApiResult.Success("Registration successful. Please check your email to verify your account.");
        }
        catch (MySqlException ex) when (ex.Number == 1062) 
        {
            logger.LogInformation("Registration failed due to duplicate email: {Email}", message.Email);
            // We don't want to let users know a duplicate email was found for security reasons.
            return ApiResult.Success("Registration successful. Please check your email to verify your account."); 
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex, "Error during user registration for email: {Email}", message.Email);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during user registration for email: {Email}", message.Email);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
    }
}