using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Models.DTO;
using DotNetAngularTemplate.Models.Responses;
using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotNetAngularTemplate.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController(ILogger<AuthController> logger, AuthService authService, EmailService emailService, EmailRateLimitService emailRateLimitService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        
            return StatusCode(StatusCodes.Status400BadRequest, new AuthResponse
            {
                Success = false,
                Message = errorMessage
            });
        }

        var result = await authService.RegisterUserAsync(requestDto.Email, requestDto.Password);

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
    public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto, [FromServices] IAntiforgery antiforgery)
    {
        if (!ModelState.IsValid)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password. Please try again."
            });
        }
        
        var userId = await authService.LoginUserAndGetIdAsync(requestDto.Email, requestDto.Password);
        if (userId == null)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password. Please try again."
            });
        }

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("UserId", userId.Value);
        SetAntiForgeryCookie(antiforgery);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful! Redirecting.."
        });
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("XSRF-TOKEN");
        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Logout successful! Redirecting.."
        });
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotEmailRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        
            return StatusCode(StatusCodes.Status400BadRequest, new AuthResponse
            {
                Success = false,
                Message = errorMessage
            });
        }

        var ip = IpHelper.GetClientIp(HttpContext);
        if (await emailRateLimitService.CanSendAsync($"forgot-password-email-by-ip-{ip}") && await emailRateLimitService.CanSendAsync($"forgot-password-email-by-email-{requestDto.Email.ToLowerInvariant()}"))
        {
            var emailResult = await emailService.SendForgotPasswordEmail(requestDto.Email);

            if (!emailResult.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse
                {
                    Success = false,
                    Message = "A server error occured. Please try again later."
                });
            }
        }
        
        return Ok(new AuthResponse
        {
            Success = true,
            Message = "If that email exists in our systems, a reset link was sent."
        });
    }

    [HttpGet("me")]
    public IActionResult Me([FromServices] IAntiforgery antiforgery)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
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