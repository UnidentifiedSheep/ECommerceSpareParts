using Core.Contracts;
using Core.Interfaces;
using MassTransit;

namespace RabbitMq.Consumers;

public class MarkupRangesChangedConsumer(IPriceSetup priceSetup) : IConsumer<MarkupRangesUpdatedEvent>
{
    public async Task Consume(ConsumeContext<MarkupRangesUpdatedEvent> context)
    {
        await priceSetup.SetupAsync();
    }
}