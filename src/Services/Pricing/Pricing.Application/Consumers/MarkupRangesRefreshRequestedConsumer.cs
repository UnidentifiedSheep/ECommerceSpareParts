using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Services;
using Application.Common.LRT;
using Contracts.Analytics;
using MassTransit;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;
using Pricing.Application.Models.Jobs;

namespace Pricing.Application.Consumers;

public class MarkupRangesRefreshRequestedConsumer(
    IMarkupInitializer markupInitializer,
    IJobService jobService,
    IJobProvider<InvalidateStalePriceOptionsLrt, NoneInputState> jobProvider
) : IConsumer<MarkupRangesRefreshRequestedEvent>
{
    public async Task Consume(ConsumeContext<MarkupRangesRefreshRequestedEvent> context)
    {
        await markupInitializer.Initialize(context.CancellationToken);

        var job = jobProvider.Create(new NoneInputState());
        await jobService.TryEnqueueJobsAsync(
            [job],
            context.CancellationToken);
    }
}
