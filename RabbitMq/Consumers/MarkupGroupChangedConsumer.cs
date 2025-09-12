using Core.Contracts;
using Core.Interfaces;
using MassTransit;

namespace RabbitMq.Consumers;

public class MarkupGroupChangedConsumer(IPriceSetup priceSetup) : IConsumer<MarkupGroupChangedEvent>
{
    public async Task Consume(ConsumeContext<MarkupGroupChangedEvent> context)
    {
        await priceSetup.SetupAsync();
    }
}
