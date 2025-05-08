using System.Security.Claims;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Authentication;

namespace DotNetAngularTemplate.Features.Auth.Handlers;

public record LoginUser(HttpContext Context, string Email, string Password);

public class LoginUserHandler(ILogger<LoginUserHandler> logger, DatabaseService databaseService, UserSessionVersionService userSessionVersionService)
{
    public async Task<Result> Handle(LoginUser message)
    {
        var user = await databaseService.GetUserByEmail(message.Email);
        if (user == null)
        {
            return Result.Failure("Invalid credentials. Please try again.");
        }
        
        var passwordValid = PasswordHelper.VerifyPassword(message.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return Result.Failure("Invalid credentials. Please try again.");
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
        
        return Result.Success("Login successful! Redirecting..");
    }
}