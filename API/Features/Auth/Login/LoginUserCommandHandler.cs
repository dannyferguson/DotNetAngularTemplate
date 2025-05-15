using System.Security.Claims;
using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Models;
using DotNetAngularTemplate.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using MySqlConnector;

namespace DotNetAngularTemplate.Features.Auth.Login;

public class LoginUserCommandHandler(ILogger<LoginUserCommandHandler> logger, DatabaseService databaseService, SessionVersionService sessionVersionService) : IRequestHandler<LoginUserCommand, ApiResult>
{
    public async Task<ApiResult> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var user = await databaseService.GetUserByEmail(command.Email, command.CancellationToken);
        if (user == null)
        {
            return ApiResult.Failure("Invalid credentials. Please try again.");
        }
        
        var passwordValid = PasswordHelper.VerifyPassword(command.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return ApiResult.Failure("Invalid credentials. Please try again.");
        }

        if (!user.EmailVerified)
        {
            return ApiResult.Failure("Email not verified. Please confirm your email before logging in.");
        }
        
        await using var unitOfWork = await databaseService.BeginUnitOfWorkAsync(command.CancellationToken);
        var ip = IpHelper.GetClientIp(command.Context);
        var saveLoginHistoryResult = await SaveLoginHistory(unitOfWork, user.Id, ip, command.CancellationToken);
        if (!saveLoginHistoryResult.IsSuccess)
        {
            await unitOfWork.RollbackAsync(command.CancellationToken);
            return ApiResult.Failure("An unexpected error occurred. Please try again later.");
        }
        await unitOfWork.CommitAsync(command.CancellationToken);
        
        var version = await sessionVersionService.GetVersionAsync(user.Id.ToString());
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, command.Email),
            new(ClaimTypes.Role, "USER"),
            sessionVersionService.CreateVersionClaim(version)
        };
        var identity = new ClaimsIdentity(claims, "AppCookie");
        var principal = new ClaimsPrincipal(identity);
        
        await command.Context.SignInAsync("AppCookie", principal);
        
        logger.LogInformation("User {Email} has logged in!", command.Email);
        
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