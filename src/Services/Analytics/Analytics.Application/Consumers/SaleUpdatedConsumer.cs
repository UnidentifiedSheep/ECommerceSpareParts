using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleUpdatedConsumer : IConsumer<SaleUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SaleUpdatedEvent> context)
    {
    }
}