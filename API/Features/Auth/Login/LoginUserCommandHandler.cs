using System.Security.Claims;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Authentication;

namespace DotNetAngularTemplate.Features.Auth.Login;

public class LoginUserCommandHandler(ILogger<LoginUserCommandHandler> logger, DatabaseService databaseService, UserSessionVersionService userSessionVersionService)
{
    public async Task<ApiResult> Handle(LoginUserCommand message)
    {
        var user = await databaseService.GetUserByEmail(message.Email);
        if (user == null)
        {
            return ApiResult.Failure("Invalid credentials. Please try again.");
        }
        
        var passwordValid = PasswordHelper.VerifyPassword(message.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return ApiResult.Failure("Invalid credentials. Please try again.");
        }
        
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
}