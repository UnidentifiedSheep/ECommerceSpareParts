using Contracts.Markup;
using MassTransit;
using Pricing.Abstractions.Interfaces.Services.Pricing;

namespace Pricing.Application.Consumers;

public class MarkupGroupChangedConsumer(IMarkupSetup markupSetup) : IConsumer<MarkupGroupChangedEvent>
{
    public async Task Consume(ConsumeContext<MarkupGroupChangedEvent> context)
    {
        await markupSetup.SetupAsync();
    }
}