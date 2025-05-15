using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Models;

namespace DotNetAngularTemplate.Features.Auth.Register;

public record CreateUserCommand(string Ip, string Email, string Password, CancellationToken CancellationToken) : IRequest<ApiResult>;