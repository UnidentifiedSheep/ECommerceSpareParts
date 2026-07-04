using Application.Common.Services.Events;
using Domain.Interfaces.Events;
using MediatR;

namespace Application.Common.Abstractions;

public abstract class BatchableDomainEventHandler<TEvent>
    : INotificationHandler<Batch<TEvent>>
    where TEvent : IBatchableDomainEvent
{
    public abstract Task Handle(Batch<TEvent> notification, CancellationToken cancellationToken);
}
