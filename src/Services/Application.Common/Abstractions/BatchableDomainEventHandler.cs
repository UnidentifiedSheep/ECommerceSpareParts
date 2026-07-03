using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Domain.Interfaces.Events;

namespace Application.Common.Abstractions;

public abstract class BatchableDomainEventHandler<TEvent> 
    : IDomainEventHandler<Batch<TEvent>> 
    where TEvent : IBatchableDomainEvent
{
    public abstract Task Handle(Batch<TEvent> notification, CancellationToken cancellationToken);
}