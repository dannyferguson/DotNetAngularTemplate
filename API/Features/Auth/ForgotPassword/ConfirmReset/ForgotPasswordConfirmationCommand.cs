using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Models;

namespace DotNetAngularTemplate.Features.Auth.ForgotPassword.ConfirmReset;

public record ForgotPasswordConfirmationCommand(
    string Ip,
    string Code,
    string Password,
    CancellationToken CancellationToken) : IRequest<ApiResult>;