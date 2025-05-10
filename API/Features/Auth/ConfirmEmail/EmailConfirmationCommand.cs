namespace DotNetAngularTemplate.Features.Auth.ConfirmEmail;

public record EmailConfirmationCommand(string Code, CancellationToken CancellationToken);