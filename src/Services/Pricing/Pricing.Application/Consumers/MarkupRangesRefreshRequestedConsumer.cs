using Application.Common.Interfaces.Services;
using Contracts.Analytics;
using MassTransit;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Models.Jobs;

namespace Pricing.Application.Consumers;

public class MarkupRangesRefreshRequestedConsumer(
    IMarkupInitializer markupInitializer,
    IJobService jobService
) : IConsumer<MarkupRangesRefreshRequestedEvent>
{
    public async Task Consume(ConsumeContext<MarkupRangesRefreshRequestedEvent> context)
    {
        await markupInitializer.Initialize(context.CancellationToken);

        var job = InvalidateStalePriceOptionsJob.Create();
        await jobService.TryEnqueueJobsAsync(
            [job],
            context.CancellationToken);
    }
}
