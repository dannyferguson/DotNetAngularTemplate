namespace DotNetAngularTemplate.Features.Auth.ForgotPassword.RequestReset;

public record ForgotPasswordCommand(string Ip, string Email);