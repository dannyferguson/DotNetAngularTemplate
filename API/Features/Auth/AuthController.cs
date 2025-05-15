using System.Security.Claims;
using DotNetAngularTemplate.Features.Auth.ConfirmEmail;
using DotNetAngularTemplate.Features.Auth.ForgotPassword.ConfirmReset;
using DotNetAngularTemplate.Features.Auth.ForgotPassword.RequestReset;
using DotNetAngularTemplate.Features.Auth.Login;
using DotNetAngularTemplate.Features.Auth.Register;
using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotNetAngularTemplate.Features.Auth;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController(
    ILogger<AuthController> logger,
    Mediator mediator) : ControllerBase
{
    // Ensure that endpoints whose response time varies depending on if an account exists or not respond within a constant amount of time in order to reduce the risk of timing based attacks.
    private const int MinimumResponseTimeInMs = 1500;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        var ip = IpHelper.GetClientIp(HttpContext);
        
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => mediator.Send(new CreateUserCommand(ip, requestDto.Email, requestDto.Password, cancellationToken), cancellationToken),
            MinimumResponseTimeInMs,
            logger);

        return result.IsSuccess ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto,
        [FromServices] IAntiforgery antiforgery, CancellationToken cancellationToken)
    {
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => mediator.Send(new LoginUserCommand(HttpContext, requestDto.Email, requestDto.Password, cancellationToken), cancellationToken),
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
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        var ip = IpHelper.GetClientIp(HttpContext);

        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => mediator.Send(new ForgotPasswordCommand(ip, requestDto.Email, cancellationToken), cancellationToken),
            MinimumResponseTimeInMs,
            logger);

        return result.IsSuccess ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpPost("forgot-password-confirmation")]
    public async Task<IActionResult> ForgotPasswordConfirmation(
        [FromBody] ForgotPasswordConfirmationRequestDto requestDto, CancellationToken cancellationToken)
    {
        var ip = IpHelper.GetClientIp(HttpContext);

        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => mediator.Send(
                new ForgotPasswordConfirmationCommand(ip, requestDto.Code, requestDto.Password, cancellationToken), cancellationToken),
            MinimumResponseTimeInMs,
            logger);

        return result.IsSuccess ? Ok(result) : Unauthorized(result);
    }
    
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] EmailConfirmationRequestDto requestDto, CancellationToken cancellationToken)
    {
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => mediator.Send(
                new EmailConfirmationCommand(requestDto.Code, cancellationToken), cancellationToken),
            MinimumResponseTimeInMs,
            logger);

        return result.IsSuccess ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
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