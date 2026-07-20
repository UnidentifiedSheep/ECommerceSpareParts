using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Pricing.Application.Interfaces.Cache;
using Pricing.Entities.DomainEvents;

namespace Pricing.Application.DomainEventHandler;

public class PriceApplierUpdatedHandler(
    IPriceApplierProvider priceApplierProvider)
    : BatchableDomainEventHandler<PriceApplierUpdatedDomainEvent>
{
    public override Task Handle(
        Batch<PriceApplierUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
        => priceApplierProvider.InvalidatePriceAppliersAsync(cancellationToken);
}
