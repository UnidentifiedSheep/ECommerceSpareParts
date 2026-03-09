using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleDeletedConsumer() : IConsumer<SaleDeletedEvent>
{
    public async Task Consume(ConsumeContext<SaleDeletedEvent> context)
    {

    }
}