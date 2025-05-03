using DotNetAngularTemplate.Models.DTO;
using DotNetAngularTemplate.Models.Responses;
using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAngularTemplate.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(ILogger<AuthController> logger, AuthService authService) : ControllerBase
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
            if (result.Error == "Email address is already registered.")
            {
                // todo email existing user to tell them someone tried to register with their email? if it was them that they're already registered and can instead just reset their password if they forgot
            }
            
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
    public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Unauthorized"
            });
        }
        
        var userId = await authService.LoginUserAndGetIdAsync(requestDto.Email, requestDto.Password);
        if (userId == null)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Unauthorized"
            });
        }

        HttpContext.Session.SetInt32("UserId", userId.Value);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful! Redirecting.."
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Logout successful! Redirecting.."
        });
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Unauthorized(); 
        }

        return Ok(new { UserId = userId });
    }
}