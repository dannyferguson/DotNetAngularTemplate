using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAngularTemplate.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(ILogger<AuthController> logger, AuthService authService) : ControllerBase
{
    public record RegisterRequest(string Email, string Password);

    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterUserAsync(request.Email, request.Password);

        if (result.IsSuccess)
        {
            return Ok(new { message = "Registration successful" });
        }
        
        if (result.Error == "Email address is already registered.")
        {
            return Conflict(new { error = result.Error });
        }

        logger.LogError("Registration failed for {@RequestEmail}: {@ResultError}", request.Email, result.Error);
        return StatusCode(StatusCodes.Status500InternalServerError,
            new { error = "Registration failed due to an unexpected error." });

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var userId = await authService.LoginUserAndGetIdAsync(request.Email, request.Password);
        if (userId == null)
            return Unauthorized();

        HttpContext.Session.SetInt32("UserId", userId.Value);

        return Ok();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized();

        return Ok(new { UserId = userId });
    }
}