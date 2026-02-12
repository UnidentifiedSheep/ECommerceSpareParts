using Contracts.Markup;
using MassTransit;
using Pricing.Abstractions.Interfaces.Services.Pricing;

namespace Pricing.Application.Consumers;

public class MarkupRangesChangedConsumer(IMarkupSetup markupSetup) : IConsumer<MarkupRangesUpdatedEvent>
{
    public async Task Consume(ConsumeContext<MarkupRangesUpdatedEvent> context)
    {
        await markupSetup.SetupAsync();
    }
}