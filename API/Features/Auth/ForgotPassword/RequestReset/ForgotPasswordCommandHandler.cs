using System.Security.Cryptography;
using DotNetAngularTemplate.Infrastructure.Models;
using DotNetAngularTemplate.Infrastructure.Services;
using MySqlConnector;

namespace DotNetAngularTemplate.Features.Auth.ForgotPassword.RequestReset;

public class ForgotPasswordCommandHandler(
    ILogger<ForgotPasswordCommandHandler> logger,
    DatabaseService databaseService,
    EmailService emailService,
    EmailRateLimitService emailRateLimitService)
{
    public async Task<ApiResult> Handle(ForgotPasswordCommand message)
    {
        await using var unitOfWork = await databaseService.BeginUnitOfWorkAsync(message.CancellationToken);
        
        try
        {
            var user = await databaseService.GetUserByEmail(message.Email, message.CancellationToken);
            if (user == null)
            {
                // We don't want to let users know if an email was found for security reasons.
                return ApiResult.Success("If that email exists in our systems, a reset link was sent.");
            }

            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); // 32 bytes = 64 hex chars
        
            await InsertPasswordResetCodeAsync(message, user.Id, code, unitOfWork);
        
            if (await emailRateLimitService.CanSendAsync($"forgot-password-email-by-ip-{message.Ip}") &&
                await emailRateLimitService.CanSendAsync($"forgot-password-email-by-email-{message.Email}"))
            {
                await emailService.SendForgotPasswordEmail(message.Email, code);
            }

            await unitOfWork.CommitAsync(message.CancellationToken);
            return ApiResult.Success("If that email exists in our systems, a reset link was sent.");
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex, "Error inserting password reset code. SQL State: {ExSqlState}, Error Code: {ExNumber}", ex.SqlState, ex.Number);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during forgot password for email: {Email}", message.Email);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
    }
    
    private async Task InsertPasswordResetCodeAsync(ForgotPasswordCommand message, int userId, string code,
        DatabaseUnitOfWork unitOfWork)
    {
        const string sql = "INSERT INTO users_password_reset_codes (user_id, code, expires_at) VALUES (@UserId, @Code, UTC_TIMESTAMP() + INTERVAL 1 HOUR)";
        var parameters = new Dictionary<string, object>
        {
            ["@UserId"] = userId,
            ["@Code"] = code
        };
        await unitOfWork.ExecuteAsync(sql, parameters, message.CancellationToken);
        logger.LogInformation("Forgot email code inserted successfully for user of id {UserId}.", userId);
    }
}