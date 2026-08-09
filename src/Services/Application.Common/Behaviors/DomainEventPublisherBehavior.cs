using Application.Common.Interfaces.Events;
using MediatR;

namespace Application.Common.Behaviors;

public class DomainEventPublisherBehavior<TRequest, TResponse>(
    IDomainEventExecutor executor
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return executor.ExecuteAsync(
            () => next(cancellationToken),
            cancellationToken);
    }
}
