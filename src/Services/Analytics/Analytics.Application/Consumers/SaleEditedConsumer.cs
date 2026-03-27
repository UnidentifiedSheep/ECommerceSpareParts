using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleEditedConsumer : IConsumer<SaleEditedEvent>
{
    public async Task Consume(ConsumeContext<SaleEditedEvent> context)
    {
    }
}