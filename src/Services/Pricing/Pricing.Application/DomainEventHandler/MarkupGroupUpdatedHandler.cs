using Application.Common.Abstractions;
using Application.Common.Services.Events;
using MediatR;
using Pricing.Entities.DomainEvents;

namespace Pricing.Application.DomainEventHandler;

public class MarkupGroupUpdatedHandler(
    ISender sender) : BatchableDomainEventHandler<MarkupGroupUpdatedDomainEvent>
{
    public override async Task Handle(Batch<MarkupGroupUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        
    }
}