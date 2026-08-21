using Application.Common.Abstractions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Settings;
using Application.Common.LRT;
using Application.Common.Services.Events;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;
using Pricing.Application.Models.Jobs;
using Pricing.Entities.DomainEvents;
using Pricing.Entities.Settings;

namespace Pricing.Application.DomainEventHandler;

public class MarkupGroupUpdatedHandler(
    ISettingsService settingsService,
    IJobService jobService,
    IJobProvider<InvalidateStalePriceOptionsLrt, NoneInputState> jobProvider)
    : BatchableDomainEventHandler<MarkupGroupUpdatedDomainEvent>
{
    public override async Task Handle(Batch<MarkupGroupUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var pricingSettings = await settingsService.GetOrDefault<PricingSetting>(cancellationToken);
        if (pricingSettings.Data.SelectedMarkupId != null && notification
            .Items
            .All(x => x.Id != pricingSettings.Data.SelectedMarkupId))
            return;

        var job = jobProvider.Create(new NoneInputState());
        await jobService.TryEnqueueJobsAsync([job], cancellationToken);
    }
}
