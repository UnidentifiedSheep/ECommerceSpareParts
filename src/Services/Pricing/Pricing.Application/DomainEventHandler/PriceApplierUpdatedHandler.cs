using Application.Common.Abstractions;
using Application.Common.Interfaces.Services;
using Application.Common.Services.Events;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Models.Jobs;
using Pricing.Entities.DomainEvents;

namespace Pricing.Application.DomainEventHandler;

public class PriceApplierUpdatedHandler(
    IPriceApplierProvider priceApplierProvider,
    IJobService jobService)
    : BatchableDomainEventHandler<PriceApplierUpdatedDomainEvent>
{
    public override async Task Handle(
        Batch<PriceApplierUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await priceApplierProvider.InvalidateConfigurationAsync(cancellationToken);

        var job = InvalidateStalePriceOptionsJob.Create();
        await jobService.TryEnqueueJobsAsync([job], cancellationToken);
    }
}
