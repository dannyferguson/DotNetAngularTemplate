namespace DotNetAngularTemplate.Infrastructure.CQRS;

public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);
}