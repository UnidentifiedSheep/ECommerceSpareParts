using Application.Common.Interfaces;
using MassTransit;
using MediatR;

namespace Application.Common.Behaviors;

public class IntegrationEventPublisherBehavior<TRequest, TResponse>(
    IIntegrationEventScope eventScope,
    IPublishEndpoint publishEndpoint) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var events = eventScope.Flush();
        foreach (var @event in events)
            await publishEndpoint.Publish(@event, cancellationToken);
        return await next(cancellationToken);
    }
}