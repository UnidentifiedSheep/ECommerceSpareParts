using Contracts.Analytics;
using MassTransit;
using Pricing.Application.Interfaces;

namespace Pricing.Application.Consumers;

public class MarkupRangesRefreshRequestedConsumer(
    IMarkupInitializer markupInitializer
) : IConsumer<MarkupRangesRefreshRequestedEvent>
{
    public async Task Consume(ConsumeContext<MarkupRangesRefreshRequestedEvent> context)
    {
        await markupInitializer.Initialize(context.CancellationToken);
    }
}