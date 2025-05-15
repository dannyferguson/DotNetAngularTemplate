using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Models;
using DotNetAngularTemplate.Infrastructure.Services;
using MySqlConnector;

namespace DotNetAngularTemplate.Features.Auth.ForgotPassword.ConfirmReset;

public class ForgotPasswordConfirmationCommandHandler(
    ILogger<ForgotPasswordConfirmationCommandHandler> logger,
    DatabaseService databaseService,
    EmailService emailService,
    EmailRateLimitService emailRateLimitService,
    SessionVersionService sessionVersionService) : IRequestHandler<ForgotPasswordConfirmationCommand, ApiResult>
{
    public async Task<ApiResult> Handle(ForgotPasswordConfirmationCommand command, CancellationToken cancellationToken)
    {
        var userId = await GetUserIdByForgotPasswordCode(command.Code, command.CancellationToken);
        if (userId == null)
        {
            logger.LogWarning("User attempted to reset password with invalid or expired code {Code}", command.Code);
            return ApiResult.Failure("This code is invalid or has expired.");
        }
        
        var passwordHash = PasswordHelper.HashPassword(command.Password);
        
        await using var unitOfWork = await databaseService.BeginUnitOfWorkAsync(command.CancellationToken);

        var updateUserPasswordResult = await databaseService.UpdateUserPassword(unitOfWork, userId.Value, passwordHash, command.CancellationToken);
        if (!updateUserPasswordResult.IsSuccess)
        {
            await unitOfWork.RollbackAsync(command.CancellationToken);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        
        var markCodeAsUsedResult = await MarkCodeAsUsed(unitOfWork, command.Code, command.CancellationToken);
        if (!markCodeAsUsedResult.IsSuccess)
        {
            await unitOfWork.RollbackAsync(command.CancellationToken);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        
        var bumpSessionVersionResult = await sessionVersionService.BumpVersionAsync(userId.Value.ToString(), unitOfWork, command.CancellationToken);
        if (!bumpSessionVersionResult.IsSuccess)
        {
            await unitOfWork.RollbackAsync(command.CancellationToken);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        
        await unitOfWork.CommitAsync(command.CancellationToken);
        
        if (await emailRateLimitService.CanSendAsync($"forgot-password-confirmation-email-by-ip-{command.Ip}") &&
            await emailRateLimitService.CanSendAsync($"forgot-password-confirmation-email-by-user-id-{userId.Value}"))
        {
            var user = await databaseService.GetUserById(userId.Value, command.CancellationToken);
            if (user != null)
            {
                await emailService.SendPasswordChangedEmail(user.Email);
            }
        }
        
        return ApiResult.Success("Password successfully reset! Redirecting you to login page.");
    }
    
    private async Task<int?> GetUserIdByForgotPasswordCode(string code, CancellationToken cancellationToken)
    {
        const string sql = "SELECT user_id FROM users_password_reset_codes WHERE code = @Code AND expires_at > @UtcNow AND used_at IS NULL";
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
    
    private async Task<ApiResult> MarkCodeAsUsed(DatabaseUnitOfWork unitOfWork, string code, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = "UPDATE users_password_reset_codes SET used_at = @UtcNow WHERE code = @Code";
            var parameters = new Dictionary<string, object>
            {
                ["@UtcNow"] = DateTime.UtcNow,
                ["@Code"] = code
            };
            
            var rowsAffected = await unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);
            if (rowsAffected == 0)
            {
                return ApiResult.Failure();
            }
            
            logger.LogInformation("Password reset code '{Code}' has successfully been used.", code);
            return ApiResult.Success();
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex,
                "Error marking password reset code {Code} as used. SQL State: {ExSqlState}, Error Code: {ExNumber}", code,
                ex.SqlState, ex.Number);
            return ApiResult.Failure();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error marking password reset code {Code} as used.", code);
            return ApiResult.Failure();
        }
    }
}