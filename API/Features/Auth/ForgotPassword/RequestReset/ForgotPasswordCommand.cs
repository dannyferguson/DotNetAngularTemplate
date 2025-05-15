using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Models;

namespace DotNetAngularTemplate.Features.Auth.ForgotPassword.RequestReset;

public record ForgotPasswordCommand(string Ip, string Email, CancellationToken CancellationToken) : IRequest<ApiResult>;