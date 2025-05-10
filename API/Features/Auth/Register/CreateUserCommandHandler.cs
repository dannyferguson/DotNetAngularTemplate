using System.Security.Cryptography;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Models;
using DotNetAngularTemplate.Infrastructure.Services;
using MySqlConnector;

namespace DotNetAngularTemplate.Features.Auth.Register;

public class CreateUserCommandHandler(ILogger<CreateUserCommandHandler> logger, DatabaseService databaseService, EmailService emailService, EmailRateLimitService emailRateLimitService)
{
    public async Task<ApiResult> Handle(CreateUserCommand message)
    {
        var passwordHash = PasswordHelper.HashPassword(message.Password);
        
        await using var unitOfWork = await databaseService.BeginUnitOfWorkAsync(message.CancellationToken);

        try
        {
            var userId = await InsertUser(message, passwordHash, unitOfWork);

            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); // 32 bytes = 64 hex chars
            
            await InsertConfirmationCode(message, userId, code, unitOfWork);

            var emailSent = await SendEmail(message.Ip, message.Email, code);
            if (!emailSent)
            {
                await unitOfWork.RollbackAsync(message.CancellationToken);
                return ApiResult.Failure("An unexpected error occurred. Please try again later.");
            }
            
            await unitOfWork.CommitAsync(message.CancellationToken);
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
            logger.LogError(ex, "Error during user registration for email: {Email}. SQL State: {ExSqlState}, Error Code: {ExNumber}", message.Email, ex.SqlState, ex.Number);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during user registration for email: {Email}", message.Email);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
    }

    private async Task InsertConfirmationCode(CreateUserCommand message, int userId, string code,
        DatabaseUnitOfWork unitOfWork)
    {
        const string insertConfirmationCodeSql = "INSERT INTO users_email_confirmation_codes (user_id, code, expires_at) VALUES (@UserId, @Code, UTC_TIMESTAMP() + INTERVAL 24 HOUR)";
        var insertConfirmationCodeParams = new Dictionary<string, object>
        {
            ["@UserId"] = userId,
            ["@Code"] = code
        };
        await unitOfWork.ExecuteAsync(insertConfirmationCodeSql, insertConfirmationCodeParams, message.CancellationToken);
        logger.LogInformation("Email confirmation code for '{Email}' inserted successfully.", message.Email);
    }

    private async Task<int> InsertUser(CreateUserCommand message, string passwordHash, DatabaseUnitOfWork unitOfWork)
    {
        const string insertUserSql = "INSERT INTO users (email, password_hash) VALUES (@Email, @Password); SELECT LAST_INSERT_ID();";
        var insertUserParams = new Dictionary<string, object>
        {
            ["@Email"] = message.Email,
            ["@Password"] = passwordHash
        };
        var userId = await unitOfWork.ExecuteScalarAsync<int>(insertUserSql, insertUserParams, message.CancellationToken);
        logger.LogInformation("User '{Email}' inserted successfully.", message.Email);
        return userId;
    }

    private async Task<bool> SendEmail(string ip, string email, string code)
    {
        if (!await emailRateLimitService.CanSendAsync($"registration-email-by-ip-{ip}") ||
            !await emailRateLimitService.CanSendAsync($"registration-email-by-email-{email}"))
        {
            return false;
        }

        return await emailService.SendRegistrationEmail(email, code);
    }
}