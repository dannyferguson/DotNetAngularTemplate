using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Models;

namespace DotNetAngularTemplate.Features.Auth.Login;

public record LoginUserCommand(HttpContext Context, string Email, string Password, CancellationToken CancellationToken) : IRequest<ApiResult>;