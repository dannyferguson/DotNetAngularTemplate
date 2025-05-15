using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Models;

namespace DotNetAngularTemplate.Features.Auth.ConfirmEmail;

public record EmailConfirmationCommand(string Code, CancellationToken CancellationToken) : IRequest<ApiResult>;