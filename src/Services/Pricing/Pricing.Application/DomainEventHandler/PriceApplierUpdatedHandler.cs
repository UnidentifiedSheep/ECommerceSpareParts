using Application.Common.Abstractions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Services;
using Application.Common.LRT;
using Application.Common.Services.Events;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;
using Pricing.Application.Models.Jobs;
using Pricing.Entities.DomainEvents;

namespace Pricing.Application.DomainEventHandler;

public class PriceApplierUpdatedHandler(
    IPriceApplierProvider priceApplierProvider,
    IJobService jobService,
    IJobProvider<InvalidateStalePriceOptionsLrt, NoneInputState> jobProvider)
    : BatchableDomainEventHandler<PriceApplierUpdatedDomainEvent>
{
    public override async Task Handle(
        Batch<PriceApplierUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await priceApplierProvider.InvalidateConfigurationAsync(cancellationToken);

        var job = jobProvider.Create(new NoneInputState());
        await jobService.TryEnqueueJobsAsync([job], cancellationToken);
    }
}
