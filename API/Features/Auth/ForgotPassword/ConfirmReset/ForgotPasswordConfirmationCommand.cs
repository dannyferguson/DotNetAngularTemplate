namespace DotNetAngularTemplate.Features.Auth.ForgotPassword.ConfirmReset;

public record ForgotPasswordConfirmationCommand(string Ip, string Code, string Password);