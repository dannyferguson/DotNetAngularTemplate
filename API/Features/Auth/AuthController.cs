using System.Security.Claims;
using DotNetAngularTemplate.Features.Auth.ForgotPassword.ConfirmReset;
using DotNetAngularTemplate.Features.Auth.ForgotPassword.RequestReset;
using DotNetAngularTemplate.Features.Auth.Login;
using DotNetAngularTemplate.Features.Auth.Register;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models;
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
    IMessageBus bus) : ControllerBase
{
    // Ensure that endpoints whose response time varies depending on if an account exists or not respond within a constant amount of time in order to reduce the risk of timing based attacks.
    private const int MinimumResponseTimeInMs = 1500;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto)
    {
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => bus.InvokeAsync<ApiResult>(new CreateUserCommand(requestDto.Email, requestDto.Password)),
            MinimumResponseTimeInMs, 
            logger);
        
        return result.IsSuccess ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto,
        [FromServices] IAntiforgery antiforgery)
    {
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => bus.InvokeAsync<ApiResult>(new LoginUserCommand(HttpContext, requestDto.Email, requestDto.Password)),
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
        return Ok(ApiResult.Success("Logout successful! Redirecting.."));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto requestDto)
    {
        var ip = IpHelper.GetClientIp(HttpContext);
        
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => bus.InvokeAsync<ApiResult>(new ForgotPasswordCommand(ip, requestDto.Email)),
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
            () => bus.InvokeAsync<ApiResult>(new ForgotPasswordConfirmationCommand(ip, requestDto.Code, requestDto.Password)),
            MinimumResponseTimeInMs,
            logger);
        
        return result.IsSuccess ? Ok(result) : Unauthorized(result);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] IAntiforgery antiforgery)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(ApiResult.Failure("Unauthorized."));
        }

        SetAntiForgeryCookie(antiforgery);

        return Ok(ApiResult.Success("Authenticated."));
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