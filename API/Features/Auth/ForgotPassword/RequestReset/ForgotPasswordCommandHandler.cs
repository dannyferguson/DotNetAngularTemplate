using System.Security.Cryptography;
using DotNetAngularTemplate.Models;
using DotNetAngularTemplate.Services;
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
        var user = await databaseService.GetUserByEmail(message.Email);
        if (user == null)
        {
            // We don't want to let users know if an email was found for security reasons.
            return ApiResult.Success("If that email exists in our systems, a reset link was sent.");
        }

        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); // 32 bytes = 64 hex chars
        
        var inserted = await InsertPasswordResetCodeAsync(user.Id, code);
        if (!inserted)
        {
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        
        if (await emailRateLimitService.CanSendAsync($"forgot-password-email-by-ip-{message.Ip}") &&
            await emailRateLimitService.CanSendAsync($"forgot-password-email-by-email-{message.Email}"))
        {
            await emailService.SendForgotPasswordEmail(message.Email, code);
        }

        return ApiResult.Success("If that email exists in our systems, a reset link was sent.");
    }

    private async Task<bool> InsertPasswordResetCodeAsync(int userId, string code)
    {
        const string sql = "INSERT INTO users_password_reset_codes (user_id, code) VALUES (@UserId, @Code)";
        var parameters = new Dictionary<string, object>
        {
            ["@UserId"] = userId,
            ["@Code"] = code
        };

        try
        {
            await databaseService.ExecuteAsync(sql, parameters);
            logger.LogInformation("Forgot email code inserted successfully for user of id {UserId}.", userId);
            return true;
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex, "Error inserting password reset code. SQL State: {ExSqlState}, Error Code: {ExNumber}", ex.SqlState, ex.Number);
            return false;
        }
    }
}