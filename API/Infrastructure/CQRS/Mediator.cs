namespace DotNetAngularTemplate.Infrastructure.CQRS;

public class Mediator(IServiceProvider serviceProvider)
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = serviceProvider.GetService(handlerType);

        if (handler == null)
            throw new InvalidOperationException($"No handler found for {request.GetType().Name}");

        return await (Task<TResponse>) handlerType
            .GetMethod("Handle")!
            .Invoke(handler, [request, ct])!;
    }
}
