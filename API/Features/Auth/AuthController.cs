using System.Security.Claims;
using DotNetAngularTemplate.Features.Auth.Handlers;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
using DotNetAngularTemplate.Models.DTO;
using DotNetAngularTemplate.Models.Responses;
using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wolverine;

namespace DotNetAngularTemplate.Features.Auth;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController(
    ILogger<AuthController> logger,
    AuthService authService,
    IMessageBus bus) : ControllerBase
{
    // Ensure that endpoints whose response time varies depending on if an account exists or not respond within a constant amount of time in order to reduce the risk of timing based attacks.
    private const int MinimumResponseTimeInMs = 1500;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto)
    {
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => bus.InvokeAsync<Result>(new CreateUser(requestDto.Email, requestDto.Password)),
            MinimumResponseTimeInMs, 
            logger);
        
        return result.IsSuccess ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto,
        [FromServices] IAntiforgery antiforgery)
    {
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => bus.InvokeAsync<Result>(new LoginUser(HttpContext, requestDto.Email, requestDto.Password)),
            MinimumResponseTimeInMs, 
            logger);

        if (!result.IsSuccess)
        {
            return Unauthorized(result);
        }
        
        SetAntiForgeryCookie(antiforgery);
        
        return Ok(result);
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("AppCookie");
        Response.Cookies.Delete("XSRF-TOKEN");
        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Logout successful! Redirecting.."
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto requestDto)
    {
        var ip = IpHelper.GetClientIp(HttpContext);
        
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => bus.InvokeAsync<Result>(new ForgotPassword(ip, requestDto.Email)),
            MinimumResponseTimeInMs, 
            logger);

        return result.IsSuccess ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpPost("forgot-password-confirmation")]
    public async Task<IActionResult> ForgotPasswordConfirmation(
        [FromBody] ForgotPasswordConfirmationRequestDto requestDto)
    {
        var ip = IpHelper.GetClientIp(HttpContext);
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => authService.UpdatePasswordIfCodeValid(requestDto.Code, requestDto.Password, ip),
            MinimumResponseTimeInMs,
            logger);

        if (!result.IsSuccess)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Unauthorized."
            });
        }

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Password successfully reset! Redirecting you to login page."
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] IAntiforgery antiforgery)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Unauthorized."
            });
        }

        SetAntiForgeryCookie(antiforgery);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Authenticated."
        });
    }

    private void SetAntiForgeryCookie(IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax,
        });
    }
}