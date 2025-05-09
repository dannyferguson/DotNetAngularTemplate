using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
using DotNetAngularTemplate.Services;

namespace DotNetAngularTemplate.Features.Auth.ForgotPassword.ConfirmReset;

public class ForgotPasswordConfirmationCommandHandler(
    ILogger<ForgotPasswordConfirmationCommandHandler> logger,
    DatabaseService databaseService,
    EmailService emailService,
    EmailRateLimitService emailRateLimitService,
    UserSessionVersionService userSessionVersionService)
{
    public async Task<ApiResult> Handle(ForgotPasswordConfirmationCommand message)
    {
        var userId = await GetUserIdByForgotPasswordCode(message.Code, message.CancellationToken);
        if (userId == null)
        {
            logger.LogWarning("User attempted to reset password with invalid or expired code {Code}", message.Code);
            return ApiResult.Failure("This code is invalid or has expired.");
        }
        
        var passwordHash = PasswordHelper.HashPassword(message.Password);

        var result = await databaseService.UpdateUserPassword(userId.Value, passwordHash, message.CancellationToken);
        if (!result.IsSuccess)
        {
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        
        await userSessionVersionService.BumpVersionAsync(userId.Value.ToString());

        if (await emailRateLimitService.CanSendAsync($"forgot-password-confirmation-email-by-ip-{message.Ip}") &&
            await emailRateLimitService.CanSendAsync($"forgot-password-confirmation-email-by-user-id-{userId.Value}"))
        {
            var user = await databaseService.GetUserById(userId.Value, message.CancellationToken);
            if (user != null)
            {
                await emailService.SendPasswordChangedEmail(user.Email);
            }
        }
        
        return ApiResult.Success("Password successfully reset! Redirecting you to login page.");
    }
    
    private async Task<int?> GetUserIdByForgotPasswordCode(string code, CancellationToken cancellationToken)
    {
        const string sql = "SELECT user_id FROM users_password_reset_codes WHERE code = @Code AND expires_at > @UtcNow";
        var parameters = new Dictionary<string, object>
        {
            ["@Code"] = code,
            ["@UtcNow"] = DateTime.UtcNow
        };

        try
        {
            var userId = await databaseService.QuerySingleAsync<int?>(sql, parameters, reader =>
            {
                var id = reader.GetInt32(0);
                return id;
            }, cancellationToken);

            return userId;
        } 
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occured while trying to retrieve user id by forgot password code {Code}", code);
            return null;
        }
    }
}