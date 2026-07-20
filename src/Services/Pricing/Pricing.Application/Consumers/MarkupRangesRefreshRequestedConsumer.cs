using Application.Common.Handlers.Jobs;
using Contracts.Analytics;
using MassTransit;
using MediatR;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Models.Jobs;

namespace Pricing.Application.Consumers;

public class MarkupRangesRefreshRequestedConsumer(
    IMarkupInitializer markupInitializer,
    ISender sender
) : IConsumer<MarkupRangesRefreshRequestedEvent>
{
    public async Task Consume(ConsumeContext<MarkupRangesRefreshRequestedEvent> context)
    {
        await markupInitializer.Initialize(context.CancellationToken);

        var job = InvalidateStalePriceOptionsJob.Create();
        await sender.Send(
            new TryEnqueueUniqJobCommand(job),
            context.CancellationToken);
    }
}
