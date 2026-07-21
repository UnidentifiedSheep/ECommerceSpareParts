using Application.Common.Abstractions;
using Application.Common.Handlers.Jobs;
using Application.Common.Services.Events;
using MediatR;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Models.Jobs;
using Pricing.Entities.DomainEvents;

namespace Pricing.Application.DomainEventHandler;

public class PriceApplierUpdatedHandler(
    IPriceApplierProvider priceApplierProvider,
    ISender sender)
    : BatchableDomainEventHandler<PriceApplierUpdatedDomainEvent>
{
    public override async Task Handle(
        Batch<PriceApplierUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await priceApplierProvider.InvalidateConfigurationAsync(cancellationToken);

        var job = InvalidateStalePriceOptionsJob.Create();
        await sender.Send(new TryEnqueueUniqJobCommand(job), cancellationToken);
    }
}
