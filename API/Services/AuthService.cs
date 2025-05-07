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
    public async Task<Result> RegisterUserAsync(string email, string password)
    {
        var hashedPassword = PasswordHelper.HashPassword(password);

        try
        {
            await dbService.InsertUserAsync(email, hashedPassword);
            return Result.Success();
        }
        catch (DuplicateEmailException)
        {
            logger.LogWarning("Registration failed due to duplicate email: {@email}", email);
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

    public async Task GenerateAndSendUserPasswordResetCode(string email, string ip)
    {
        var user = await dbService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return;
        }

        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); // 32 bytes = 64 hex chars
        await dbService.InsertPasswordResetCodeAsync(user.Value.Id, code);

        if (await emailRateLimitService.CanSendAsync($"forgot-password-email-by-ip-{ip}") &&
            await emailRateLimitService.CanSendAsync($"forgot-password-email-by-email-{email}"))
        {
            await emailService.SendForgotPasswordEmail(email, code);
        }
    }
    
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

    public async Task<int?> LoginUserAndGetIdAsync(string email, string password)
    {
        var user = await dbService.GetUserByEmailAsync(email);
        if (user == null)
            return null;

        var passwordValid = PasswordHelper.VerifyPassword(password, user.Value.PasswordHash);
        return passwordValid ? user.Value.Id : null;
    }
}