using System.Security.Cryptography;
using DotNetAngularTemplate.Exceptions;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
using MySqlConnector;

namespace DotNetAngularTemplate.Services;

public class AuthService(
    ILogger<AuthService> logger,
    DatabaseService dbService,
    EmailService emailService,
    EmailRateLimitService emailRateLimitService,
    UserSessionVersionService userSessionVersionService)
{
    public async Task<Result> UpdatePasswordIfCodeValid(string code, string password, string ip)
    {
        var userId = await dbService.GetUserIdByForgotPasswordCode(code);
        if (userId == null)
        {
            return Result.Failure("Code does not exist or has already been used.");
        }
        
        var hashedPassword = PasswordHelper.HashPassword(password);

        await dbService.UpdateUserPassword(userId.Value, hashedPassword);
        await userSessionVersionService.BumpVersionAsync(userId.Value.ToString());

        if (await emailRateLimitService.CanSendAsync($"forgot-password-confirmation-email-by-ip-{ip}") &&
            await emailRateLimitService.CanSendAsync($"forgot-password-confirmation-email-by-user-id-{userId}"))
        {
            var email = await dbService.GetEmailByUserId(userId.Value);
            if (email != null)
            {
                await emailService.SendPasswordChangedEmail(email);
            }
        }
        
        return Result.Success();
    }
}