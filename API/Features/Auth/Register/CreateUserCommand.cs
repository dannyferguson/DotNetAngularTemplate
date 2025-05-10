namespace DotNetAngularTemplate.Features.Auth.Register;

public record CreateUserCommand(string Ip, string Email, string Password, CancellationToken CancellationToken);