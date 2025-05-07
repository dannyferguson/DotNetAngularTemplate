using System.Security.Claims;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models.DTO;
using DotNetAngularTemplate.Models.Responses;
using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotNetAngularTemplate.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController(ILogger<AuthController> logger, AuthService authService, UserSessionVersionService userSessionVersionService) : ControllerBase
{
    // Ensure that endpoints whose response time varies depending on if an account exists or not respond within a constant amount of time in order to reduce the risk of timing based attacks.
    private const int MinimumResponseTimeInMs = 1500;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto)
    {
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(() =>
            authService.RegisterUserAsync(requestDto.Email, requestDto.Password), MinimumResponseTimeInMs, logger);

        if (result.IsSuccess || result.Error == "Email address is already registered.")
        {
            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Registration successful. Please check your email to verify your account."
            });
        }

        logger.LogError("Registration failed for {@RequestEmail}: {@ResultError}", requestDto.Email, result.Error);
        return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse
        {
            Success = false,
            Message = "A server error occured. Please try again later."
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto,
        [FromServices] IAntiforgery antiforgery)
    {
        var userId = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => authService.LoginUserAndGetIdAsync(requestDto.Email, requestDto.Password), MinimumResponseTimeInMs,
            logger);
        if (userId == null)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password. Please try again."
            });
        }
        
        var version = await userSessionVersionService.GetVersionAsync(userId.Value.ToString());
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new(ClaimTypes.Email, requestDto.Email),
            new(ClaimTypes.Role, "USER"),
            userSessionVersionService.CreateVersionClaim(version)
        };
        var identity = new ClaimsIdentity(claims, "AppCookie");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("AppCookie", principal);
        
        SetAntiForgeryCookie(antiforgery);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful! Redirecting.."
        });
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
        await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => authService.GenerateAndSendUserPasswordResetCode(requestDto.Email, ip), MinimumResponseTimeInMs,
            logger);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "If that email exists in our systems, a reset link was sent."
        });
    }
    
    [HttpPost("forgot-password-confirmation")]
    public async Task<IActionResult> ForgotPasswordConfirmation([FromBody] ForgotPasswordConfirmationRequestDto requestDto)
    {
        var ip = IpHelper.GetClientIp(HttpContext);
        var result = await TimingProtectorHelper.RunWithMinimumDelayAsync(
            () => authService.UpdatePasswordIfCodeValid(requestDto.Code, requestDto.Password, ip), MinimumResponseTimeInMs,
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