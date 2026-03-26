using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleCreatedConsumer : IConsumer<SaleCreatedEvent>
{
    public async Task Consume(ConsumeContext<SaleCreatedEvent> context)
    {
    }
}