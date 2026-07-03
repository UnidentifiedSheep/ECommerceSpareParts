using Application.Common.Interfaces.Events;
using MediatR;

namespace Application.Common.Behaviors;

public class DomainEventPublisherBehavior<TRequest, TResponse>(
    IDomainEventScope eventScope,
    IPublisher publisher
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        var events = eventScope.Flush();
        foreach (var @event in events)
            await publisher.Publish(@event, cancellationToken);

        return response;
    }
}