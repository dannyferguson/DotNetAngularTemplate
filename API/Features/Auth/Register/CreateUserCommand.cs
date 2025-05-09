namespace DotNetAngularTemplate.Features.Auth.Register;

public record CreateUserCommand(string Email, string Password, CancellationToken CancellationToken);