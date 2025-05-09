namespace DotNetAngularTemplate.Features.Auth.Login;

public record LoginUserCommand(HttpContext Context, string Email, string Password);