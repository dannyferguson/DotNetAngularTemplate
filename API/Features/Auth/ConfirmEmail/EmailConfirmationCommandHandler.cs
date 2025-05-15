using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Models;
using DotNetAngularTemplate.Infrastructure.Services;
using MySqlConnector;

namespace DotNetAngularTemplate.Features.Auth.ConfirmEmail;

public class EmailConfirmationCommandHandler(
    ILogger<EmailConfirmationCommandHandler> logger,
    DatabaseService databaseService) : IRequestHandler<EmailConfirmationCommand, ApiResult>
{
    public async Task<ApiResult> Handle(EmailConfirmationCommand command, CancellationToken cancellationToken)
    {
        var userId = await GetUserIdByConfirmEmailCode(command.Code, command.CancellationToken);
        if (userId == null)
        {
            logger.LogWarning("User attempted to confirm email with invalid or expired code {Code}", command.Code);
            return ApiResult.Failure("This link is invalid or has expired.");
        }
        
        await using var unitOfWork = await databaseService.BeginUnitOfWorkAsync(command.CancellationToken);
        
        var verifyUserEmailResult = await VerifyUserEmail(unitOfWork, userId.Value, command.CancellationToken);
        if (!verifyUserEmailResult.IsSuccess)
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
        
        await unitOfWork.CommitAsync(command.CancellationToken);
        return ApiResult.Success("Email confirmed! Redirecting you to login page.");
    }
    
    private async Task<int?> GetUserIdByConfirmEmailCode(string code, CancellationToken cancellationToken)
    {
        const string sql = "SELECT user_id FROM users_email_confirmation_codes WHERE code = @Code AND expires_at > @UtcNow AND confirmed_at IS NULL";
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
        catch (MySqlException ex)
        {
            logger.LogError(ex,
                "Error retrieving user id by confirm email code {Code}. SQL State: {ExSqlState}, Error Code: {ExNumber}", code,
                ex.SqlState, ex.Number);
            return null;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occured while trying to retrieve user id by confirm email code {Code}", code);
            return null;
        }
    }
    
    private async Task<ApiResult> VerifyUserEmail(DatabaseUnitOfWork unitOfWork, int userId, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = "UPDATE users SET email_verified = 1, updated_at = @UtcNow WHERE id = @UserId";
            var parameters = new Dictionary<string, object>
            {
                ["@UtcNow"] = DateTime.UtcNow,
                ["@UserId"] = userId,
            };
            await unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);
            logger.LogInformation("User by id '{Id}' has successfully confirmed their email.", userId);
            return ApiResult.Success();
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex,
                "Error confirming email for user id {UserId}. SQL State: {ExSqlState}, Error Code: {ExNumber}", userId,
                ex.SqlState, ex.Number);
            return ApiResult.Failure();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error confirming email for user of id: {UserId}", userId);
            return ApiResult.Failure();
        }
    }
    
    private async Task<ApiResult> MarkCodeAsUsed(DatabaseUnitOfWork unitOfWork, string code, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = "UPDATE users_email_confirmation_codes SET confirmed_at = @UtcNow WHERE code = @Code";
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
            
            logger.LogInformation("Email confirmation code '{Code}' has successfully been used.", code);
            return ApiResult.Success();
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex,
                "Error marking email confirmation code {Code} as used. SQL State: {ExSqlState}, Error Code: {ExNumber}", code,
                ex.SqlState, ex.Number);
            return ApiResult.Failure();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error marking email confirmation code {Code} as used.", code);
            return ApiResult.Failure();
        }
    }
}