using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Events;
using MassTransit;
using MediatR;

namespace Application.Common.Behaviors;

public class IntegrationEventPublisherBehavior<TRequest, TResponse>(
    IIntegrationEventScope eventScope,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork? unitOfWork = null
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
        foreach (var @event in events) await publishEndpoint.Publish(@event, cancellationToken);

        if (events.Count != 0 && unitOfWork is { Context.SuppressAutoSave: false })
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
