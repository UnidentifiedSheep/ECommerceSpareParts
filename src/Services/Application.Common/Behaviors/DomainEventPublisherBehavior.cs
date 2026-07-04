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
    private const int MaxDispatchRounds = 10;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var _ = eventScope.EnableCollection();
        var response = await next(cancellationToken);

        for (var round = 0; round < MaxDispatchRounds; round++)
        {
            var events = eventScope.Flush();
            if (events.Count == 0) return response;

            foreach (var @event in events)
                await publisher.Publish((object)@event, cancellationToken);
        }

        throw new InvalidOperationException(
            $"Domain event dispatch exceeded {MaxDispatchRounds} rounds for request {typeof(TRequest).Name}.");
    }
}
