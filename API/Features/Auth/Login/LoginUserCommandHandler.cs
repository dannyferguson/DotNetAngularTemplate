using System.Security.Claims;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Models;
using DotNetAngularTemplate.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using MySqlConnector;

namespace DotNetAngularTemplate.Features.Auth.Login;

public class LoginUserCommandHandler(ILogger<LoginUserCommandHandler> logger, DatabaseService databaseService, UserSessionVersionService userSessionVersionService)
{
    public async Task<ApiResult> Handle(LoginUserCommand message)
    {
        var user = await databaseService.GetUserByEmail(message.Email, message.CancellationToken);
        if (user == null)
        {
            return ApiResult.Failure("Invalid credentials. Please try again.");
        }
        
        var passwordValid = PasswordHelper.VerifyPassword(message.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return ApiResult.Failure("Invalid credentials. Please try again.");
        }

        if (!user.EmailVerified)
        {
            return ApiResult.Failure("Email not verified. Please confirm your email before logging in.");
        }
        
        await using var unitOfWork = await databaseService.BeginUnitOfWorkAsync(message.CancellationToken);
        var ip = IpHelper.GetClientIp(message.Context);
        var saveLoginHistoryResult = await SaveLoginHistory(unitOfWork, user.Id, ip, message.CancellationToken);
        if (!saveLoginHistoryResult.IsSuccess)
        {
            await unitOfWork.RollbackAsync(message.CancellationToken);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        await unitOfWork.CommitAsync(message.CancellationToken);
        
        var version = await userSessionVersionService.GetVersionAsync(user.Id.ToString());
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, message.Email),
            new(ClaimTypes.Role, "USER"),
            userSessionVersionService.CreateVersionClaim(version)
        };
        var identity = new ClaimsIdentity(claims, "AppCookie");
        var principal = new ClaimsPrincipal(identity);
        
        await message.Context.SignInAsync("AppCookie", principal);
        
        logger.LogInformation("User {Email} has logged in!", message.Email);
        
        return ApiResult.Success("Login successful! Redirecting..");
    }
    
    private async Task<ApiResult> SaveLoginHistory(DatabaseUnitOfWork unitOfWork, int userId, string ip, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = "INSERT INTO users_login_history (user_id, ip_address) VALUES (@UserId, @Ip)";
            var parameters = new Dictionary<string, object>
            {
                ["@UserId"] = userId,
                ["@Ip"] = ip
            };
            
            var rowsAffected = await unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);
            if (rowsAffected == 0)
            {
                return ApiResult.Failure();
            }
            
            logger.LogInformation("Saved login for user with id '{UserId}'.", userId);
            return ApiResult.Success();
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex,
                "Error saving login for {UserId} with ip {Ip}. SQL State: {ExSqlState}, Error Code: {ExNumber}", userId, ip,
                ex.SqlState, ex.Number);
            return ApiResult.Failure();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error saving login for {UserId} with ip {Ip}.", userId, ip);
            return ApiResult.Failure();
        }
    }
}